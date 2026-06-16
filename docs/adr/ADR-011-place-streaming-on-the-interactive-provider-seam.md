# ADR-011: Extend IChatModelProvider with Streaming Rather Than Add a Parallel Seam

## Status

Accepted

## Date

2026-06-04

## Related

- ADR-005-introduce-provider-abstraction-for-claude-integration.md (the interactive seam this decision extends)
- ADR-010-introduce-parallel-batch-provider-abstraction.md (the inverse case — same seam test, opposite verdict)
- ADR-009-implement-prompt-caching-inside-provider-boundary.md (restraint precedent: don't manufacture abstraction one example can't justify)
- ADR-006 / ADR-008 (telemetry pattern reused with stream-specific span names)

## Context

Day 5 (ADR-005) established IChatModelProvider as the single seam for the
interactive LLM path, with one synchronous operation:

    ```csharp
    public interface IChatModelProvider
    {
        Task<ChatResponse> SendAsync(ChatRequest request, CancellationToken cancellationToken = default);
    }
    ```

One request in, one completion out, interactive latency. ChatRequest and ChatResponse
are provider-agnostic and have been protected from provider-specific leakage since
Day 5.

Day 8 (ADR-010) added the Message Batches API as a parallel seam,
`IBatchChatModelProvider`, on Interface-Segregation + Liskov grounds: batch introduced
a genuinely new lifecycle (submit -> poll -> retrieve), returned no synchronous
`ChatResponse`, and could not be implemented by a non-batch provider without throwing
`NotSupportedException`. Forcing those methods onto the interactive seam would have
broken substitutability and burdened interactive callers with operations they never
invoke. So batch split out.

Day 9 adds Server-Sent-Events streaming to the interactive path: incremental token
delivery via `POST /api/ai/chat/stream`, so time-to-first-token becomes the governing
latency metric. The new operation:

    ```csharp
    IAsyncEnumerable<ChatChunk> StreamAsync(ChatRequest request, CancellationToken ct);
    ```

where `ChatChunk` is a new provider-agnostic delta contract (text delta, nullable stop
reason, nullable end-of-stream usage).

The question Day 9 must answer is NOT how to stream — it is where the streaming
operation lives: extended onto `IChatModelProvider`, or split into a third sibling seam
`(IStreamingChatModelProvider)` mirroring `IBatchChatModelProvider`. The parallel to
ADR-010 is superficially obvious — "we split batch, so split streaming too, for
consistency." That reflex is the trap. ADR-010's split was principled because of what
the test found, not because splitting is inherently tidy. The same test must be re-run
on streaming, and the verdict must be allowed to come out opposite if that's where the
test points.

### The test being applied (carried forward verbatim from ADR-010)

A change earns a new seam when it alters either the operation set or the
substitutability contract — concretely, when extending the existing interface would
force a Liskov violation (a provider lacking the feature must throw) or an
Interface-Segregation violation (consumers depend on operations they never call).

| Change | New lifecycle / operation? | Returns synchronous `ChatResponse`? | Substitutable for a provider lacking it? | Verdict |
|---|---|---|---|---|
| Caching (Day 7, ADR-009) | No | Yes | Yes | Inside the seam |
| Batch (Day 8, ADR-010) | Yes (submit/poll/retrieve) | No | No — must throw NotSupportedException | New seam |
| Streaming (Day 9) | No — same request->completion, delivered incrementally | No (`IAsyncEnumerable`) | Yes — degrades to a single terminal chunk | Extend the seam |

The middle column is a decoy. Both batch and streaming return something other than a
synchronous `ChatResponse`, so "doesn't return `ChatResponse`" cannot be the deciding
factor — if it were, streaming would split too. The load-bearing column is the last
one: substitutability. That is where batch and streaming diverge, and it is the entire
reason the verdicts differ.

- Batch breaks Liskov. A provider with no batch endpoint has nowhere to submit a job;
  SubmitBatchAsync/GetBatchStatusAsync/GetBatchResultsAsync have no honest
  implementation but throw. Substitutability is broken at the type level.
- Streaming preserves Liskov. A provider that cannot stream natively implements
  StreamAsync by calling its own SendAsync and yielding the completion as one terminal
  ChatChunk. That is graceful degradation, not an exception — the caller still gets
  every token, just in one frame instead of many. Full substitutability holds.

The honest nuance, stated so a reviewer can't ambush it: StreamAsync IS a new method.
But the seam abstracts the operation set and the substitutability contract, not the
method count (ADR-010's own framing). Streaming adds a method without adding a new
lifecycle and without breaking substitutability. Batch added methods AND a lifecycle
AND broke substitutability. Method count alone never decided ADR-009 or ADR-010; it
does not decide this one.

### Interface Segregation, checked honestly

ISP was a real cost for batch: AiController (interactive) would have been forced
to depend on submit/poll/retrieve it never calls. For streaming, the consumer is the
same interactive controller serving the same interactive path — buffered and streamed
chat are two delivery modes of one user-facing operation, consumed together, not by
disjoint clients. There is no segregation pressure to relieve. Splitting would create
an artificial boundary, not honor a real one.

## Decision

We will extend IChatModelProvider with StreamAsync:

    public interface IChatModelProvider
    {
        Task<ChatResponse> SendAsync(ChatRequest request, CancellationToken cancellationToken = default);
        IAsyncEnumerable<ChatChunk> StreamAsync(ChatRequest request, CancellationToken ct);
    }

Specifically:

- StreamAsync returns `IAsyncEnumerable<ChatChunk>`. ChatRequest is reused unchanged as
  input — a request authored for the buffered path streams without modification.
- ChatChunk is a new provider-agnostic contract in Models/AI/: text delta, nullable stop
  reason, nullable end-of-stream Usage. No Anthropic SSE event types leak into it.
- The Liskov-safety guarantee (degrade-to-single-chunk for a non-streaming provider)
  is encoded explicitly. Two acceptable encodings, decided in Build mode: a default
  interface implementation on IChatModelProvider that wraps SendAsync (structural
  guarantee, weaker discoverability), or a per-provider implementation (explicit,
  slightly more boilerplate). Either keeps substitutability real, not conventional.
- IChatModelProvider, ChatRequest, ChatResponse keep their existing shape; SendAsync
  is untouched. No new DI seam, no new registration.
- The HTTP surface is an explicit `POST /api/ai/chat/stream` endpoint, not content
  negotiation on the existing endpoint (see Alternative 3).

### Why the same YAGNI question reaches the opposite conclusion from ADR-010

ADR-009 and ADR-010 are the same discipline: match the abstraction weight to the
actual contract change — no more, no less. ADR-009 declined a decorator because one
provider couldn't justify it (don't over-abstract). ADR-010 added a seam because the
contract change genuinely broke substitutability (don't under-abstract a real break).
ADR-011 is ADR-009's side of that coin applied to seams: a parallel
IStreamingChatModelProvider would be abstraction weight the substitutability math does
not require — the speculative over-abstraction YAGNI exists to prevent. The symmetry is
exact: ADR-010 rejected extending the seam because extension broke the contract;
ADR-011 rejects splitting the seam because the split is unjustified by the contract.
Opposite structural outcomes, identical test, because the underlying contract changes
are not alike. That is what makes the 009/010/011 set principled rather than three
independent style calls.

**Side-by-side reconciliation — batch (ADR-010) vs streaming (ADR-011):**

| Decision variable | Batch (ADR-010) | Streaming (ADR-011) |
| --- | --- | --- |
| New lifecycle? | Yes — submit → poll → retrieve | No — one request, one completion |
| Breaks Liskov? | Yes — non-batch provider must throw | No — degrade to single terminal chunk |
| ISP pressure? | Yes — interactive controller must not depend on submit/poll/retrieve | No — same controller serves buffered and streamed chat |
| Verdict | New seam: `IBatchChatModelProvider` | Extend existing: `IChatModelProvider` |

The two decisions are explicit inverses on every dimension. Any future operation should
be run through the same four-column test, not pattern-matched to either verdict by
analogy. Batch and streaming share nothing except that both involve `IAsyncEnumerable`
somewhere in the implementation — which is irrelevant to the seam decision.

## Alternatives Considered

### Alternative 1 — Extend IChatModelProvider with StreamAsync

This is the chosen alternative. See Decision above.

Why chosen: Streaming is the interactive chat operation with incremental delivery. It
adds no lifecycle, returns the same logical completion, and preserves substitutability
via graceful degradation. Both the Liskov and ISP costs that forced batch out are
absent. Extending the seam is the minimum-weight change that is still correct.

Consequences accepted: the interface grows from one operation to two; any future
non-streaming provider carries a trivial degrade implementation; existing test doubles
must add StreamAsync.

### Alternative 2 — New IStreamingChatModelProvider parallel seam (mirror IBatchChatModelProvider)

A third sibling interface, registered separately in DI, ClaudeStreamingChatModelProvider
implementing it, mirroring the batch pattern exactly.

Why rejected:

- The parallelism to batch is cosmetic, not structural. Batch earned its seam by
  failing the substitutability test; streaming passes it. Copying the form of ADR-010
  while ignoring why ADR-010 reached its verdict is cargo-culting the conclusion
  without the reasoning.
- It manufactures an Interface-Segregation boundary where no consumer split exists.
  The same controller serves buffered and streamed chat; forcing them behind two
  interfaces makes callers resolve two seams to serve one user-facing operation.
- It is the over-abstraction ADR-009 explicitly warned against: an interface with one
  implementation and one consumer, whose shape is fitted to a single example, added
  for symmetry rather than need.

What would change to revisit: a provider appears whose streaming semantics diverge
structurally from buffered completion — e.g. interleaved tool-call or multi-modal
events that cannot be modeled as ChatChunk text-or-usage deltas. That would be a
genuine operation-set change (a new lifecycle of partial events), would likely break
the single-ChatChunk degrade path, and would re-open the split on the same test that
closes it today.

### Alternative 3 — Content negotiation on the existing POST /api/ai/chat (no interface change)

One endpoint; the Accept header (application/json vs text/event-stream) selects
buffered or streamed response. Argued to need no interface change at all.

Why rejected:

- It does not actually answer the seam question — it relocates it. The provider still
  must expose some way to produce an `IAsyncEnumerable<ChatChunk>` distinct from
  `Task<ChatResponse>;` you cannot unify the two return shapes without either buffering
  the stream (defeating streaming) or making the buffered path an async enumerable
  (complicating the common case for the rare one). Content negotiation is an HTTP-layer
  concern layered on top of a seam decision, not a substitute for it.
- An explicit /chat/stream endpoint is more observable and more discoverable: it gets
  its own operation name in telemetry (TTFT belongs only to the stream path), its own
  Swagger contract, and its own resilience/timeout profile, without Accept-header
  branching inside one action. Conflating both modes behind one route muddies the very
  latency signal Day 9 exists to expose.

What would change to revisit: a client constraint that forbids a second route (e.g. a
fixed API contract that only permits header-based negotiation). Even then, content
negotiation would sit on top of Alternative 1 — it is orthogonal to, not a replacement
for, extending the seam.

## Consequences

### Positive

- One seam for the interactive operation; callers reason about "chat" in a single
  place, buffered or streamed.
- Substitutability is preserved and made explicit via the degrade-to-single-chunk
  contract — future providers stream natively or degrade gracefully, never throw.
- No DI proliferation: no new registration, no second controller dependency to
  resolve for one user-facing operation.
- Demonstrates the ADR-010 seam test is principled, not decorative — the same test
  yields the opposite verdict, which is only possible if the test was real.
- ChatChunk reuse of ChatRequest keeps request authoring identical across buffered and
  streamed paths.

### Negative

- IChatModelProvider grows from one operation to two. A hypothetical future provider
  that cannot stream still must supply a StreamAsync (the degrade wrapper). The cost is
  small — the wrapper is trivial and correct — but it is a non-zero ISP pressure that
  did not exist on the Day-5 single-method seam.
- Every existing test double / mock of IChatModelProvider must now implement
  StreamAsync. Bounded (one real implementation plus mocks), but it is a
  compile-breaking interface change for test code.
- ChatChunk is a new provider-agnostic contract that must be policed against Anthropic
  SSE-type leakage, the same vigilance ChatResponse already requires.

### Neutral / Tradeoffs

- The explicit-endpoint-vs-content-negotiation choice (Alternative 3) is decided in
  favor of an explicit route, but that is an HTTP-surface decision orthogonal to the
  seam; it could be revisited without reopening this ADR.
- The degrade-path encoding (default interface method vs per-provider implementation)
  is deferred to Build mode. Default interface methods give a structural guarantee but
  hurt discoverability; per-provider is explicit but repeats boilerplate. Either
  satisfies the substitutability contract.
- Telemetry reuses the Day 6 two-span pattern with new names (ai.chat.stream,
  claude.chat.stream.api) and a new TTFT histogram. Stream latency semantics differ
  from buffered: TTFT is the governing metric, total duration is secondary — the
  interactive 5xx/latency alerting still applies (unlike batch), but TTFT gets its own
  percentile SLO.
- Prompt caching (ADR-009) must continue to function on the stream path:
  cache_read_input_tokens arrives in the message_start.usage SSE frame and must still
  surface on the span. This is a verification obligation, not a design change.

## Implementation Notes

### Files affected

- `src/lab-observability-api/Services/AI/IChatModelProvider.cs`
  - Add `IAsyncEnumerable<ChatChunk> StreamAsync(ChatRequest request, CancellationToken ct);`
  - Optionally add a default interface implementation wrapping SendAsync for the
    degrade guarantee.
- `src/lab-observability-api/Models/AI/ChatChunk.cs` (new)
  - Provider-agnostic delta: text delta, nullable stop reason, nullable end-of-stream
    Usage. No Anthropic types.
- `src/lab-observability-api/Services/Claude/ClaudeApiClient.cs`
  - Add the streaming transport path: `"stream": true` payload (carrying cache_control
    identically to the buffered path), SSE parse (message_start →
    content_block_delta/text_delta → message_delta → message_stop), yield return per
    delta, CancellationToken propagated to the upstream HTTP read so client disconnect
    cancels the Anthropic call.
- `src/lab-observability-api/Services/AI/ClaudeChatModelProvider.cs`
  - Implement StreamAsync; outer ai.chat.stream span tags llm.provider, llm.model
    (Day 6 two-span pattern preserved).
- `src/lab-observability-api/Controllers/AiController.cs`
  - Add `POST /api/ai/chat/stream` (`[HttpPost("chat/stream")]`): Content-Type: text/event-stream,
    Cache-Control: no-cache, X-Accel-Buffering: no, response buffering disabled,
    await foreach writing `data:` frames + flush per chunk, HttpContext.RequestAborted as
    the token, `event: error` + correlationId on mid-stream failure.
- `src/lab-observability-api/Telemetry/GatewayTelemetry.cs`
  - Add `Histogram<double> StreamTtftMs` (`ai.provider.stream.ttft_ms`); add ai.chat.stream /
    claude.chat.stream.api ActivitySource spans; tags llm.stream.ttft_ms,
    llm.stream.chunks, and end-of-stream llm.tokens.*and llm.cache.*.

### Files explicitly NOT affected

- `src/lab-observability-api/Models/AI/ChatRequest.cs` — reused as stream input, unmodified.
- `src/lab-observability-api/Models/AI/ChatResponse.cs` — buffered contract, unchanged.
- `src/lab-observability-api/Services/AI/IBatchChatModelProvider.cs` and the batch
  provider — orthogonal; batch does not learn about streaming.

### Migration steps

- Interface change is compile-breaking for all IChatModelProvider implementers and
  mocks. Real implementers: ClaudeChatModelProvider (one). Update it plus any test
  doubles in the same change. If using a default interface method for the degrade path,
  mocks compile without change but should still be exercised.
- No DI registration change: StreamAsync rides the existing scoped IChatModelProvider
  registration (`AddScoped<IChatModelProvider, ClaudeChatModelProvider>()`).
- No new app setting anticipated (streaming reuses Anthropic__*); confirmed in
  `Infra/Day-009/appsettings-template.md` ("No new app settings this day").

### Rollback strategy

- Purely additive at the HTTP surface: the /chat/stream endpoint can be removed without
  touching SendAsync or the buffered /chat path. Removing StreamAsync from the interface
  is a larger revert (touches the interface + implementer + mocks); prefer disabling the
  endpoint over reverting the seam if a fast rollback is needed.

## References

- ADR-005 (the interactive seam this extends)
- ADR-009 (restraint precedent: don't over-abstract one example) and ADR-010 (the
  inverse seam decision — same test, opposite verdict)
- ADR-006 / ADR-008 (telemetry foundations reused with stream-specific span names)
- Anthropic streaming Messages documentation: <https://docs.anthropic.com/en/docs/build-with-claude/streaming>
  (verify current SSE event names and the message_start/message_delta usage fields
  during Build mode)
- `docs/standards/kql-cookbook.md` — TTFT percentile query to be added on Day 9
