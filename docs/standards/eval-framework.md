# Evaluation Framework Standard

**Phase:** 1 (foundation) / Phase 2 (full implementation)
**Applies to:** measuring and maintaining LLM response quality across model versions, prompt changes, and provider swaps

---

## Why evaluation is an architectural concern

Prompt changes, model upgrades, and provider swaps all change LLM output quality in ways that unit tests cannot detect. An evaluation framework is the safety net for the non-deterministic layer — it catches quality regressions the way tests catch code regressions.

Without an eval framework:

- A model upgrade silently changes response quality
- A prompt change that improves cost breaks accuracy
- A provider swap that reduces latency reduces helpfulness
- You have no data to defend a technical decision to a CEO

---

## Evaluation dimensions

Every LLM response must eventually be measurable on at least four dimensions:

| Dimension | What it measures | How to measure |
|---|---|---|
| **Correctness** | Did the response answer the question accurately? | LLM-as-judge or golden set comparison |
| **Relevance** | Is the response on-topic and useful? | LLM-as-judge scoring (1-5) |
| **Groundedness** | Does the response make claims the context supports? (RAG) | Citation verification |
| **Safety** | Does the response violate content policy? | Rule-based + LLM-as-judge |
| **Latency** | Is the response fast enough for the use case? | TTFT + total duration from telemetry |
| **Cost** | Is the token spend within budget? | Cache hit rate + token counts from telemetry |

Phase 1 instruments latency and cost via telemetry (done). Correctness, relevance, and safety are Phase 2.

---

## Evaluation architecture (Phase 2 target)

```text
[Test dataset]
      ↓
[Gateway /api/ai/chat or /stream]
      ↓
[Response collector]  →  [Output store: Azure Blob or Cosmos]
                                ↓
                        [Evaluator service]  ←  [IChatModelProvider (LLM-as-judge)]
                                ↓
                        [Score store + dashboard]
```

The evaluator service calls the gateway again, passing the original prompt, the response, and a grading rubric to the LLM. This is **LLM-as-judge**: using a capable model (claude-opus-4-8) to grade the outputs of a cheaper model (claude-haiku-4-5-*).

---

## Golden test set

A golden test set is a fixed collection of (prompt, expected_response) or (prompt, grading_rubric) pairs. It must:

- Cover the primary use cases the gateway serves
- Include edge cases: very short prompts, very long prompts, ambiguous prompts, safety-probing prompts
- Be versioned in `docs/evaluations/` (Phase 2 folder to create)
- Never change without a review — it is the benchmark, not the subject of optimization

Minimum size: 25 examples at Phase 2 start. Grow to 100+ before Phase 3.

---

## LLM-as-judge pattern

```csharp
// Evaluator prompt template (stored in config, not hard-coded)
const string JudgePrompt = """
You are evaluating an AI assistant response.

Original question: {question}
Assistant response: {response}

Rate the response on:
1. Correctness (1-5): Is the information accurate?
2. Relevance (1-5): Does it address what was asked?
3. Clarity (1-5): Is it clearly written?

Return ONLY a JSON object: {"correctness": N, "relevance": N, "clarity": N, "reasoning": "..."}
""";
```

Use `claude-opus-4-8` as judge for Phase 2 evaluations. Use structured output / JSON mode to ensure parseable scores.

---

## Regression evaluation gates

Before any of the following actions, run the golden test set and confirm scores do not drop more than 10% on any dimension:

- Upgrading the model version (e.g., `claude-sonnet-4-6` → new version)
- Changing the system prompt
- Adding or modifying `cache_control` annotations
- Switching providers (e.g., Claude → Azure OpenAI)
- Changing `MaxTokens`

If scores drop > 10%: the change is blocked until the cause is identified. The evaluation results go in the day's `05-audit-log.md`.

---

## Phase 1 foundation (build now)

The full evaluator service is Phase 2. In Phase 1, establish:

- [ ] `docs/evaluations/` folder created with a `golden-set.md` stub (minimum 5 examples)
- [ ] `ChatResponse` includes enough metadata to reconstruct what happened (model, tokens, cache hit)
- [ ] A manual eval checklist in `02-completion-checklist.md` for any day that changes the system prompt or model

---

## Observability integration

Evaluation scores are first-class telemetry in Phase 2. Add to `GatewayTelemetry.cs`:

```csharp
public Histogram<double> EvalCorrectnessScore { get; }   // ai.eval.correctness
public Histogram<double> EvalRelevanceScore { get; }     // ai.eval.relevance
public Histogram<double> EvalClarityScore { get; }       // ai.eval.clarity
```

KQL query (add to `kql-cookbook.md` in Phase 2): eval score trend over time, broken down by prompt template version.

---

## Tools ecosystem awareness

Evaluators currently used in industry (for reference — not adding as dependencies yet):

| Tool | Use case | Notes |
|---|---|---|
| **Langfuse** | Open-source LLM observability + eval | Self-hostable; strong tracing + scoring |
| **Braintrust** | Evaluation platform | Strong golden-set management |
| **Azure AI Evaluation SDK** | Microsoft-native eval | Integrates with Azure AI Foundry |
| **Promptfoo** | Prompt regression testing | CLI-first, works with any provider |
| **LlamaIndex Eval** | RAG-specific evaluation | Grounded response scoring |

When Phase 2 begins: evaluate Langfuse (open-source, self-hostable) vs. Azure AI Evaluation SDK (Microsoft-native) for this gateway's evaluation layer. That ADR is a Phase 2 day-one task.
