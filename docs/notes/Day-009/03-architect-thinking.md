# Day 9 — Architect Thinking

## 1. The core decision: extend IChatModelProvider vs a new IStreamingChatModelProvider

Day 8 introduced `IBatchChatModelProvider` as a sibling seam. The tempting pattern is to do the same for streaming: `IStreamingChatModelProvider` sitting alongside `IChatModelProvider`. The right answer is the opposite — `StreamAsync` goes on `IChatModelProvider`. Here's the asymmetry and why it matters.

**Why batch earned its own seam (ADR-010):**
Batch has a genuinely different lifecycle — submit, poll, retrieve — with its own status model, its own error semantics (a submitted batch cannot be safely retried), and its own SLO class (minutes to hours, not milliseconds). A batch job is not a chat request; it is a named, persistent resource with server-side state. These differences are deep enough that a single interface cannot express both semantically.

**Why streaming does NOT earn its own seam:**
Streaming is the same operation as synchronous chat — one request, one response — with a different delivery mechanism. The caller sends `"What's the capital of France?"` and receives text back. Whether that text arrives in one block or in 40 chunks is a transport detail, not a semantic difference. The conceptual operation is identical; only the response shape changes. A `StreamAsync` method on `IChatModelProvider` expresses this correctly: "this interface does interactive chat, and you can consume the response as a stream or as a complete object."

**The alternative that was rejected:**
`IStreamingChatModelProvider` would force callers to register and resolve two interfaces for the same logical provider, create a seam boundary that carries no semantic weight, and require every future provider (Azure OpenAI, Bedrock) to implement two separate registrations for what is fundamentally one capability. It would also undermine the portability guarantee of the abstraction — the point of `IChatModelProvider` is that callers don't know which LLM they're talking to. Splitting the interface defeats that.

**Ruling question for future seam decisions:**
Does this operation have a genuinely different lifecycle and error model, or is it the same operation with a different transport/encoding? If different lifecycle → new seam. If same operation → extend the existing one.

---

## 2. SSE parsing in .NET — what to watch for

Anthropic's streaming response is `text/event-stream` (Server-Sent Events). The wire format is line-based:

```text
event: content_block_start
data: {"type":"content_block_start","index":0,"content_block":{"type":"text","text":""}}

event: content_block_delta
data: {"type":"content_block_delta","index":0,"delta":{"type":"text_delta","text":"Hello"}}

event: message_delta
data: {"type":"message_delta","delta":{"stop_reason":"end_turn"},"usage":{"output_tokens":12}}

event: message_stop
data: {"type":"message_stop"}
```

Key implementation notes:

- Read the response stream line by line; blank line = end of event
- Only `content_block_delta` events with `delta.type = "text_delta"` carry text to yield
- `message_delta` carries `stop_reason` and final `output_tokens` — yield the final `ChatChunk` with usage here
- `message_stop` is the terminal signal; close the stream cleanly
- Input token count is in the initial `message_start` event: `message.usage.input_tokens`
- The `HttpClient` timeout must be `Timeout.InfiniteTimeSpan` — the streaming client inherits this from `ClaudeApiClient`'s HttpClient registration; do NOT set a short timeout on the streaming path

**Azure App Service SSE pass-through:**
App Service on Linux (the lab's tier) passes SSE through without modification as long as:

- `Cache-Control: no-cache` is set (prevents proxy buffering)
- `X-Accel-Buffering: no` is set (nginx directive — App Service uses nginx as reverse proxy)
- Response flushing is explicit: call `HttpResponse.Body.FlushAsync()` after each chunk

If the streaming works locally but chunks arrive all at once from Azure, missing `X-Accel-Buffering: no` is almost always the cause.

---

## 3. No resilience pipeline on the streaming client

The synchronous `ClaudeApiClient` has `AddStandardResilienceHandler`. The streaming path must NOT retry on the SSE connection because:

1. Streaming starts emitting content immediately — a retry after partial output would duplicate tokens
2. If a stream is interrupted mid-way (500 or network drop), the caller has already received partial content; the correct recovery is caller-side re-connection with a `Last-Event-ID`, not a transparent retry at the gateway layer

This is a different flavor of the batch no-retry rule (ADR-010), but for a different reason: batch avoids retry to prevent duplicate submissions; streaming avoids retry to prevent duplicate token delivery.

---

## 4. First-token latency as the primary SLO metric for streaming

For synchronous chat, the relevant metric is total latency (time to complete response). For streaming, total latency is less meaningful — users don't wait for the full response, they wait for the first token. `StreamFirstTokenMs` is the metric that directly correlates with perceived performance. A p95 first-token latency of < 500ms is the target for interactive use.

This distinction matters architecturally: if you only instrument `ProviderLatencyMs` (total), you will miss regressions in first-token time that users actually feel.

---

## CEO Framing

Streaming turns "wait 4 seconds, then read" into "read as it arrives." That is not a performance optimization — it is the difference between a product that feels alive and one that feels like a form submission. Any AI feature demo that does not stream will be compared unfavorably to ChatGPT by every non-technical stakeholder in the room; any production deployment that does not stream will see lower engagement and higher perceived latency in every user study. The architectural cost of adding streaming after the fact (retrofitting the seam, updating every caller, re-testing the pipeline) is 3–5× higher than building it correctly the first time.

---

## 5. What the wrong answer looks like

Returning a fully buffered response with `Content-Type: text/event-stream` — accumulating all chunks in memory, then emitting them all at once in a single flush. This is technically "streaming" by content type but produces the same blank-then-all UX as synchronous. The test for real streaming: first token in the console output must appear before the model has finished generating, observable with a prompt that produces a long response.
