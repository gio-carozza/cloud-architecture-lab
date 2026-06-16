# Multi-Turn Context Management Standard

**Phase:** 1 — Day 10 scope
**Applies to:** any endpoint or feature that maintains conversation history across turns

---

## The problem

LLMs are stateless. Every API call starts with a blank slate. Multi-turn conversation requires the gateway to reconstruct the full conversation history on every request — which means managing what gets included, how long it's kept, and what gets dropped when the context window fills.

Doing this wrong causes: broken conversations, silent context truncation, ballooning token costs, and prompt injection via injected history.

---

## Context window budget

Anthropic Claude context limits (current as of Phase 1):

| Model | Context window | Practical input budget (leaving room for output) |
|---|---|---|
| `claude-sonnet-4-6` | 200,000 tokens | ~190,000 tokens |
| `claude-haiku-4-5-*` | 200,000 tokens | ~190,000 tokens |
| `claude-opus-4-8` | 200,000 tokens | ~190,000 tokens |

Even at 200K tokens, unconstrained history will eventually exceed the budget. Design for truncation from Day 1.

---

## Conversation history contract

History is represented as a sequence of turns, each with a role and content:

```csharp
public record ConversationTurn(string Role, string Content);
// Role: "user" or "assistant" — never "system" (system prompt is separate)
```

`ChatRequest` gains a `History` property in Day 10:

```csharp
public class ChatRequest
{
    [Required]
    [StringLength(10000, MinimumLength = 1)]
    public string Prompt { get; set; } = string.Empty;

    public IReadOnlyList<ConversationTurn> History { get; set; } = [];
}
```

The `History` list is the caller's responsibility to maintain and pass on each request. The gateway does not store session state — it is stateless by design. Callers hold the history client-side and send it with each turn.

---

## Truncation strategy

When the sum of `History` tokens + system prompt tokens + `Prompt` tokens + `MaxTokens` (reserved for output) exceeds the model's context window, the gateway must truncate.

**Truncation rule: drop oldest turns first, always in pairs (user + assistant).**

Never drop a user turn without its corresponding assistant turn — orphaned turns break the alternating role requirement of the Anthropic API.

```csharp
// Pseudocode — implement in ClaudeApiClient.BuildAnthropicRequest()
while (TotalTokenEstimate(systemPrompt, history, prompt) + MaxTokens > ContextLimit)
{
    if (history.Count < 2) break; // cannot truncate further
    history.RemoveRange(0, 2);    // remove oldest user+assistant pair
}
```

Token estimation: use character count / 4 as a fast approximation (4 chars ≈ 1 token). For precise counts, use the Anthropic `count_tokens` endpoint — but only for long histories (> 50 turns) where estimation error is significant.

---

## System prompt position

The system prompt is always first, before history. It is the part most likely to be cached (cache_control applied here). Do not include the system prompt in the `History` list.

```json
{
  "system": [{ "type": "text", "text": "...", "cache_control": { "type": "ephemeral", "ttl": "1h" } }],
  "messages": [
    { "role": "user",      "content": "Turn 1 user" },
    { "role": "assistant", "content": "Turn 1 assistant" },
    { "role": "user",      "content": "Current prompt" }
  ]
}
```

---

## Context poisoning prevention

History provided by the client is untrusted input. Before appending history to the request:

- Validate each turn has `role` in `["user", "assistant"]` — reject `"system"` role injections
- Validate content length per turn: reject any single turn > 50,000 characters
- Validate alternating roles: the sequence must alternate user/assistant, starting with user
- Strip any `cache_control` annotations from client-supplied history — only the gateway controls caching

Return `400 VALIDATION_FAILED` with `code: "INVALID_HISTORY"` if any check fails.

---

## Prompt caching with history

Prompt caching applies to the system prompt only. Do NOT apply `cache_control` to conversation history turns — history changes every turn, defeating the cache. Caching a changing sequence causes `cache_creation_input_tokens` charges on every turn with zero hits.

---

## Telemetry additions (Day 10)

Add to the existing `claude.chat.api` span:

| Tag | Value |
|---|---|
| `llm.context.history_turns` | Number of turns passed in |
| `llm.context.turns_truncated` | Number of turns dropped before sending |
| `llm.context.estimated_tokens` | Pre-request token estimate |

---

## Storage strategy (future — Phase 2)

The current stateless design puts history management on the caller. For Phase 2, when the gateway serves real users, add server-side session storage:

- Azure Cosmos DB or Azure Cache for Redis — keyed by `sessionId`
- `sessionId` passed as a header or query param; the gateway hydrates history server-side
- TTL on sessions: 30 minutes of inactivity (configurable via `SessionOptions`)
- History stored encrypted at rest if it may contain PII

This is an ADR-worthy decision when Phase 2 begins. Do not implement session storage in Phase 1.
