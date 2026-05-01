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

## Day 6 Complete
Serilog as logger, OpenTelemetry as the export pipeline (see ADR-006, ADR-008).
Correlation middleware, resilience pipeline, structured error handling, and
LLM-specific Activity instrumentation all in place. Deploy verification deferred
until Azure CLI is restored.
See ../../docs/architecture/observability-architecture.md (forthcoming) for
current state.