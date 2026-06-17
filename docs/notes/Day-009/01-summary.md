# Day 009 — Streaming Responses on the Interactive Path

## Track

Build

## Career Phase

AI Engineer (Phase 1)

## Focus

Add Server-Sent Events (SSE) streaming to the interactive chat path: a new
`POST /api/ai/chat/stream` endpoint, a StreamAsync operation on the provider
seam, and a first-token-latency (TTFT) histogram so perceived latency becomes a
first-class, queryable SLO — not a feeling.

## Why This Matters

Interactive LLM UX lives or dies on time-to-first-token, not total completion
time. A 6-second buffered response feels broken; the same 6 seconds with tokens
arriving at 400ms feels instant. Every production chat surface (Claude, ChatGPT,
Copilot) streams — a gateway that only returns buffered completions is
structurally incapable of backing a real interactive product. This is the day the
gateway stops being a request/response demo and starts being able to sit behind a
live UI. It also forces the first honest re-application of Day 8's seam test: the
discipline only counts if it's applied when the answer is inconvenient, not just
when it's tidy.

## Whose Problem Am I Solving?

### Collaboration Lens (Day 009)

**Primary — DevOps / SRE**
Posture: good citizen — brief the new streaming failure modes before they surface in an incident
Today's question: can you tell from the logs whether a latency complaint is a TTFT problem or a total-duration problem without calling me?

**10yo:** Today we made the gateway answer like a typewriter instead of a printer — words appear one at a time — and we added a stopwatch on the very first word so anyone watching can tell if the AI is slow to start or slow to finish.
**CEO:** The TTFT histogram means the first latency complaint from a customer demo is diagnosable from a dashboard — and that distinction (provider slow vs. model slow) determines whether we escalate to Anthropic or optimize our prompt.
**Engineer:** `IAsyncEnumerable<ChatChunk>`, TTFT stopwatch, X-Accel-Buffering:no, RequestAborted propagation, SSE event:error frame — each is an on-call artifact as much as a feature; the histogram is what turns "it felt slow" into "p95 TTFT spiked at 14:32."
**Architect:** Streaming adds a failure mode sync paths don't have — the half-delivered response; the mid-stream error contract (event:error frame, correlationId only, never stack trace) is the architectural answer to "what does on-call see when the provider drops the stream at token 47?"

**Also in frame:**

- Security/AppSec/CISO — mid-stream error frame carries only correlationId + safe message; stack trace suppression on the streaming path requires the same discipline as the sync path, confirmed in the Day 9 pillars audit
- Cloud & Model-Vendor Support — Anthropic SSE format (message_start / content_block_delta / message_delta / message_stop) is vendor-specific; the parser must handle all four event types or silently drop data

A user staring at a spinner. Concretely: the future Phase 2 customer demo where a
non-technical stakeholder types a question and watches the answer appear. If the
first token takes three seconds, the demo is dead before the content matters.
TTFT is their experience rendered as a number I can alert on. Secondary human:
future-me-on-call, who needs to know whether a latency complaint is a TTFT problem
(provider/streaming) or a total-duration problem (model/length) — two different
fixes, indistinguishable without the histogram.

## What I Will Build

- StreamAsync on IChatModelProvider returning `IAsyncEnumerable<ChatChunk>`
  (placement decision formalized in ADR-011 — see Architect Thinking).
- ChatChunk — a provider-agnostic delta contract (text delta, nullable stop
  reason, optional end-of-stream usage). Reuses ChatRequest unchanged as input.
- ClaudeApiClient streaming path: "stream": true, SSE event parsing
  (message_start → content_block_delta/text_delta → message_delta → message_stop),
  with cancellation propagated to the upstream HTTP call.
- `POST /api/ai/chat/stream` on AiController: text/event-stream response,
  response buffering disabled, HttpContext.RequestAborted wired through to cancel
  the Anthropic stream on client disconnect.
- TTFT histogram (ai.provider.stream.ttft_ms) on GatewayTelemetry, plus a
  claude.chat.stream.api span following the Day 6 two-span pattern.
- Mid-stream error contract: once the 200 + SSE headers are sent, failures emit an
  SSE event: error frame with a correlationId — never a half-written body, never
  a stack trace.
- Verification that prompt caching (ADR-009) still functions on the stream path —
  cache_read_input_tokens arrives in message_start.usage and must still surface.

## Step-by-Step Execution

### Phase A — Contract & seam (the ADR-011 decision point)

- Add StreamAsync(ChatRequest, CancellationToken) : `IAsyncEnumerable<ChatChunk>`
  to IChatModelProvider.
- Add the ChatChunk model (provider-agnostic; no Anthropic types).
- Do NOT touch ChatRequest (reused as-is) or ChatResponse.

### Phase B — Provider streaming path

- ClaudeApiClient: build the streaming payload (carry cache_control exactly as
  the buffered path does), open the SSE stream, parse deltas, yield chunks.
- Start the TTFT stopwatch at request send; record the histogram the instant the
  first text_delta is yielded.
- Accumulate output tokens from message_delta.usage; surface input + cache tokens
  from message_start.usage on the span.
- The Claude provider's StreamAsync orchestrates; tags llm.provider, llm.model on
  the outer span (Day 6 pattern preserved).

### Phase C — SSE endpoint

- `POST /api/ai/chat/stream` on AiController: set Content-Type: text/event-stream,
  Cache-Control: no-cache, X-Accel-Buffering: no; disable response buffering.
- await foreach over StreamAsync, writing data: frames and flushing per chunk.
- Wire HttpContext.RequestAborted as the CancellationToken.
- Emit event: error + correlationId on mid-stream failure; close with a terminal
  frame on success.

### Phase D — Telemetry

- GatewayTelemetry: add `Histogram<double>` StreamTtftMs and the
  claude.chat.stream.api / ai.chat.stream spans; tag llm.stream.ttft_ms,
  llm.stream.chunks, and end-of-stream llm.tokens.*+ llm.cache.*.

### Phase E — Verify (local → Azure)

- Local: stream a >=1100-token-system-prompt request; confirm tokens arrive
  incrementally, TTFT recorded, cache_read tokens > 0 on the second call.
- Deploy via `/deploy`; confirm SSE is not buffered by App Service.
- KQL: TTFT percentile query against the new histogram.

## Architect Thinking

The load-bearing decision is not "how to stream" — it's "does streaming belong on
IChatModelProvider or on a sibling seam?" Day 8 answered the equivalent question
for batch with a sibling interface (IBatchChatModelProvider) on
Interface-Segregation + Liskov grounds. Intellectual honesty demands streaming run
through the same test, not assume the answer:

| Change | New operation set? | Returns synchronous ChatResponse? | Substitutable for a provider lacking it? | Verdict |
|---|---|---|---|---|
| Caching (Day 7, ADR-009) | No | Yes | Yes | Inside the seam |
| Batch (Day 8, ADR-010) | Yes (submit/poll/retrieve) | No | No (NotSupportedException) | New seam |
| Streaming (Day 9) | Same operation, different delivery | No (IAsyncEnumerable) | Yes — every real LLM provider streams; buffered-only degrades to a single-chunk stream | On the seam (lean) |

The distinction that flips streaming away from batch's verdict: batch changed the
interaction model (job-shaped, asynchronous over minutes/hours, no synchronous
completion object). Streaming preserves the interaction model — one request, one
logical completion, interactive latency — and changes only the delivery mechanics
(incremental vs buffered). Crucially, Liskov holds: a provider that can't natively
stream can implement StreamAsync by yielding its buffered answer as one terminal
chunk. That is a graceful degradation, not a NotSupportedException — the exact break
batch could not avoid. So streaming reads more like caching (stayed in) than like
batch (split out).

The alternative being rejected (and the revisit condition): a separate
IStreamingChatModelProvider. Rejected because it would force interactive callers to
choose an interface by delivery mechanic rather than by operation, and because the
substitutability cost batch paid simply isn't present here. Revisit if a provider
appears whose streaming semantics diverge structurally from buffered completion
(e.g. tool-call streaming with interleaved non-text events that can't be modeled as
ChatChunk deltas). That would be a real operation-set change and would re-open the
split.

Common beginner mistakes this day must avoid: (1) trying to change HTTP status after
the SSE stream has opened — impossible; errors must become SSE frames; (2) forgetting
that App Service / intermediaries buffer SSE by default, silently defeating the
feature while "working" locally; (3) leaking the upstream stream when the client
disconnects — RequestAborted must cancel the Anthropic call or you pay for tokens
nobody reads.

### CEO Framing

This is the difference between a chatbot that feels instant and one that feels
broken — and it gives us a single number (time-to-first-token) we can put an SLA on,
so "the AI is slow" becomes a measurable, defensible commitment instead of an
argument.

### Phase Note

Reinforces Phase 1 (AI Engineer): streaming/SSE is a core API-integration skill on
the AI-102 path, and TTFT instrumentation extends the "token cost and latency as
first-class telemetry" muscle from Days 6-8. It also pre-stages Phase 2 — a
streaming endpoint is the minimum bar for any live customer demo.

## Artifacts

- Code:
  - `Services/AI/IChatModelProvider.cs` (add StreamAsync)
  - `Models/AI/ChatChunk.cs` (new)
  - `Services/Claude/ClaudeApiClient.cs` (streaming path)
  - `Services/AI/ClaudeChatModelProvider.cs` (StreamAsync)
  - `Controllers/AiController.cs` (new /chat/stream action)
  - `Telemetry/GatewayTelemetry.cs` (TTFT histogram + spans)
- Docs:
  - `docs/adr/ADR-011-place-streaming-on-the-interactive-provider-seam.md`
  - `docs/architecture/day-009-streaming-responses-on-interactive-path.md`
  - `docs/standards/kql-cookbook.md` (TTFT percentile query)
  - `docs/notes/Day-009/` (summary, checklist, architect-thinking, posture-check, files-changed)
- Infra:
  - `Infra/Day-009/appsettings-template.md` (no new app settings — streaming reuses existing Anthropic__* config)

## Portfolio Value

Proves to a hiring panel that I can (1) implement production SSE streaming in .NET 8
with correct cancellation and mid-stream error semantics, (2) instrument perceived
latency as an SLO, not just total duration, and — most credibly — (3) re-apply my own
architectural test to a borderline case and reach a defensible verdict opposite to
the prior one. That last point is what separates "follows patterns" from "owns the
reasoning behind them."

## Completion Checklist

See 02-completion-checklist.md.

## Certification Reinforcement

- AZ-900: Secondary — Azure Monitor custom metrics/histograms; App Service as the
  compute host for a streaming workload.
- AZ-104: None — no admin/ops surface today (AZ-104 parallel track restructured to start ~Day 035).
- AZ-305: Secondary — designing for performance and latency SLOs; choosing the right
  response pattern (streaming vs buffered) for an interactive workload; monitoring design.
- AI-102: Primary — implementing generative AI solutions with streaming completions;
  monitoring and optimizing AI solutions (latency/cost telemetry). AI-102 retires
  June 30, 2026 — keep this track moving.

## Architect Posture Check

See 04-posture-check.md (filled at end of day, BEFORE marking complete).
