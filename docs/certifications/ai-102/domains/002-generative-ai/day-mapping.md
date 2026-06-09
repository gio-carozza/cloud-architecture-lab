# Day Mapping — Implement Generative AI Solutions (AI-102 Domain 2)

| Day | Topics Covered |
|-----|----------------|
| Day-008 | Batch API pattern (submit/poll/retrieve); Azure OpenAI Global Batch quota model (separate enqueued-token quota, 50% cost reduction); batch vs. synchronous routing decision; hard-cap budget enforcement at submission boundary; provider abstraction (`IBatchProvider`) enabling Anthropic ↔ Azure OpenAI swap |
| Day-009 | SSE streaming completions (`stream: true`); SSE event parsing (message_start / content_block_delta / message_delta / message_stop); time-to-first-token (TTFT) as primary streaming SLO; `X-Accel-Buffering: no` for nginx proxy buffering disable; client disconnect → upstream cancellation via RequestAborted; mid-stream error contract (event:error frame); provider seam design decision (extend IChatModelProvider vs new seam — Liskov substitutability test) |
