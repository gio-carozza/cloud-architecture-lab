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

## Day 6 In Progress
Adding: Serilog, Application Insights, correlation middleware, resilience pipeline.
See ../../.claude/skills/observability-net/SKILL.md