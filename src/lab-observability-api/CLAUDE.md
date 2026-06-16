# Lab.Observability.Api

The .NET 8 AI Gateway. This is the anchor workload of the entire roadmap.

## Boundaries

- This service exposes provider-agnostic chat APIs over HTTP.
- It does NOT implement business logic on top of LLMs — it's the gateway.
- Future services (RAG, agents) will call this service, not Anthropic directly.

## Local Development

- API key: `dotnet user-secrets set "Anthropic:ApiKey" "sk-..."`
- Run: `dotnet run`
- Swagger: `https://localhost:7XXX/swagger`

## Code Rules (see ../../.claude/skills/dotnet-api-conventions/SKILL.md)

- `IChatModelProvider` is the seam — never bypass it
- Options pattern with `ValidateOnStart()`
- Structured logging only
- Never return stack traces

## Days 6–9 Complete

Day 6: Serilog + OpenTelemetry export pipeline (ADR-006, ADR-008), correlation
middleware, resilience pipeline, structured error handling, LLM Activity spans.
Day 7: Prompt caching inside the provider boundary (ADR-009), CacheHits/CacheMisses
counters, system prompt as cacheable content array with TTL.
Day 8: Parallel batch provider seam (ADR-010), IBatchChatModelProvider,
ClaudeBatchApiClient, AiBatchController, batch telemetry, MaxBatchSize cap.
Day 9: SSE streaming on interactive path (ADR-011), StreamAsync on IChatModelProvider,
ChatChunk/ChatChunkUsage models, ClaudeApiClient.StreamChatAsync, StreamTtftMs histogram.
See ../../docs/architecture/observability-architecture.md for current state.
