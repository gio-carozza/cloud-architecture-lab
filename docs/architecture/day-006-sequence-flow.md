# Day 006 — Sequence Flow

## Happy path

Client
  |
  | POST /api/ai/chat
  v
CorrelationIdMiddleware
  |
  | Ensure x-correlation-id exists
  v
AiController
  |
  | Validate request prompt
  v
IChatModelProvider
  |
  | Resolve concrete provider implementation
  v
ClaudeChatModelProvider
  |
  | Build Claude payload
  v
ClaudeApiClient
  |
  | POST <https://api.anthropic.com/v1/messages>
  v
Anthropic Claude API
  |
  | Return response JSON
  v
ClaudeApiClient
  |
  | Extract text / classify response
  v
ClaudeChatModelProvider
  |
  | Build ChatResponse
  v
AiController
  |
  | Return HTTP 200
  v
Client

## Failure path — provider non-success

Client
  |
  | POST /api/ai/chat
  v
CorrelationIdMiddleware
  v
AiController
  v
ClaudeChatModelProvider
  v
ClaudeApiClient
  |
  | POST /v1/messages
  v
Anthropic Claude API
  |
  | Return 4xx / 5xx
  v
ClaudeApiClient
  |
  | Throw ClaudeProviderException
  v
Global Exception Middleware
  |
  | Map exception to stable ApiError JSON
  v
Client

## Failure path — timeout / circuit breaker

Client
  |
  | POST /api/ai/chat
  v
AiController
  v
ClaudeChatModelProvider
  v
ClaudeApiClient
  |
  | Outbound provider call goes slow or repeated failures occur
  v
Resilience Pipeline
  |
  | Timeout rejection or open circuit
  v
ClaudeApiClient
  |
  | Throw ClaudeProviderException
  v
Global Exception Middleware
  |
  | Return 503 or 504 with correlation ID
  v
Client

## Observability flow

Request enters app
  |
  | CorrelationIdMiddleware assigns request ID
  v
Logs include request scope
  |
  | Controller and provider log key events
  v
ClaudeApiClient emits provider metrics/traces
  |
  | Azure Monitor / Application Insights receives telemetry
  v
Operator can search using correlation ID, latency, failure count, and status patterns

## Notes

- Correlation IDs are added at the beginning of the pipeline.
- Provider failures are normalized before leaving the API.
- Readiness is based on configuration presence, not a live provider dependency check.
- Day 6 prioritizes safe operability over feature expansion.
