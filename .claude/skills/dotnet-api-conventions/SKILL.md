---
name: dotnet-api-conventions
description: Conventions for adding code to Lab.Observability.Api — the .NET 8 AI Gateway. Use when writing controllers, services, providers, options classes, or middleware in src/lab-observability-api. Enforces the provider abstraction, configuration binding, and error handling rules.
allowed-tools: Read, Write
---

# Lab.Observability.Api Conventions

## When to use

- Adding any new code to `src/lab-observability-api/`
- Creating controllers, services, options classes, middleware
- Implementing a new `IChatModelProvider`
- Reviewing existing code for compliance

## Namespace & Folder Layout

- Root namespace: `Lab.Observability.Api`
- Folder = namespace segment:
  - `Controllers/` → `Lab.Observability.Api.Controllers`
  - `Services/AI/` → `Lab.Observability.Api.Services.AI` (provider interfaces + implementations)
  - `Services/Claude/` → `Lab.Observability.Api.Services.Claude` (Anthropic transport)
  - `Models/` → `Lab.Observability.Api.Models`
  - `Options/` → `Lab.Observability.Api.Options`
  - `Middleware/` → `Lab.Observability.Api.Middleware`
  - `Telemetry/` → `Lab.Observability.Api.Telemetry`

## Provider Abstraction (CRITICAL — do not break)

The seam is `IChatModelProvider`. ALL LLM calls go through it.

```csharp
public interface IChatModelProvider
{
    Task<ChatResponse> SendAsync(ChatRequest request, CancellationToken cancellationToken = default);

    // Default implementation degrades to a single terminal ChatChunk — non-streaming providers
    // get substitutability for free. Override in ClaudeChatModelProvider for real SSE streaming.
    IAsyncEnumerable<ChatChunk> StreamAsync(ChatRequest request, CancellationToken ct)
    {
        // default: call SendAsync, yield result as one terminal chunk
    }
}
```

**Rules:**

- `ChatRequest` and `ChatResponse` are PROVIDER-AGNOSTIC. No Anthropic-,
  OpenAI-, or Bedrock-specific types may leak into them.
- Provider-specific quirks live INSIDE the provider implementation only.
- New providers (Azure OpenAI, Bedrock, Foundry) implement this interface
  without changing the contract.
- DI registration is keyed by provider name for future routing:
  `services.AddKeyedSingleton<IChatModelProvider, ClaudeChatModelProvider>("claude");`

## Options Pattern (configuration binding)

```csharp
// Options/AnthropicOptions.cs
public sealed class AnthropicOptions
{
    public const string SectionName = "Anthropic";
    public required string ApiKey { get; init; }
    public required string Model { get; init; }
    public required string BaseUrl { get; init; }
    public int MaxTokens { get; init; } = 1024;
}
```

**Registration in Program.cs:**

```csharp
builder.Services
    .AddOptions<AnthropicOptions>()
    .Bind(builder.Configuration.GetSection(AnthropicOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();
```

**Consumption:**

```csharp
public ClaudeChatModelProvider(IOptions<AnthropicOptions> options, ...)
{
    _options = options.Value;
}
```

**Always:** `using Microsoft.Extensions.Options;` (the `IOptions<T>` gotcha).

## Secrets Handling

| Environment | Mechanism | Key Format |
|---|---|---|
| Local dev | `dotnet user-secrets` | `Anthropic:ApiKey` (colon) |
| Azure App Service | App Settings env var | `Anthropic__ApiKey` (double underscore) |
| Production (future) | Azure Key Vault reference | `@Microsoft.KeyVault(...)` |

**Never:** put secrets in `appsettings.json` or `appsettings.Development.json`.

## Controller Pattern

```csharp
[ApiController]
[Route("api/[controller]")]
public class AiController : ControllerBase
{
    private readonly IChatModelProvider _provider;
    private readonly ILogger<AiController> _logger;

    public AiController(IChatModelProvider provider, ILogger<AiController> logger)
    {
        _provider = provider;
        _logger = logger;
    }

    [HttpPost("chat")]
    public async Task<ActionResult<ChatResponse>> Chat(
        [FromBody] ChatRequest request,
        CancellationToken ct)
    {
        var response = await _provider.SendAsync(request, ct);
        return Ok(response);
    }
}
```

**Rules:**

- Controllers are THIN. No business logic. Delegate to providers/services.
- Always accept `CancellationToken` — pass through to async calls.
- Return `ActionResult<T>` for typed responses + status code flexibility.
- Validate via model attributes; don't manually if-check.

## Error Handling (production-grade)

**Never return stack traces.** The actual implementation is a global exception
pipeline registered as an inline `app.Use(...)` delegate in `Program.cs` (not a
separate middleware class) — it catches `ClaudeProviderException` first (classified,
provider-specific) then falls through to a generic `Exception` handler:

```csharp
// Program.cs — global exception handling pipeline
app.Use(async (context, next) =>
{
    try { await next(); }
    catch (ClaudeProviderException ex)
    {
        var correlationId = context.GetCorrelationId();
        // ... LogWarning with Provider/ProviderStatusCode/ProviderErrorCode/IsTransient
        context.Response.StatusCode = ex.ProviderStatusCode switch
        {
            HttpStatusCode.TooManyRequests => StatusCodes.Status503ServiceUnavailable,
            HttpStatusCode.RequestTimeout => StatusCodes.Status504GatewayTimeout,
            _ when ex.IsTransient => StatusCodes.Status503ServiceUnavailable,
            _ => StatusCodes.Status502BadGateway
        };
        await context.Response.WriteAsJsonAsync(new ApiError(Code: "claude_provider_error", Message: "...", CorrelationId: correlationId));
    }
    catch (Exception ex)
    {
        var correlationId = context.GetCorrelationId();
        // ... LogError, then write a generic ApiError with the same correlationId, no stack trace
    }
});
```

**Provider-specific errors** (e.g., 401 from Anthropic) are classified into
`ClaudeProviderException` (see `Services/Claude/ClaudeProviderException.cs`) and
translated to safe HTTP status codes in the pipeline above, not surfaced raw.

## Logging Conventions

- Use structured logging only: `_logger.LogInformation("Sent {Tokens} to {Provider}", tokens, name);`
- Never log secrets, API keys, or full prompt bodies in production.
- Include correlation ID in every log scope.
- Day 6 added Serilog + Application Insights — already wired; use `ILogger<T>` and the existing telemetry helpers.

## Common mistakes (avoid)

- Putting Anthropic SDK types in `ChatRequest`/`ChatResponse` (breaks abstraction)
- Reading config via `IConfiguration` directly instead of `IOptions<T>` (no validation)
- Returning `Exception.Message` to clients (info disclosure)
- Forgetting `CancellationToken` (resource leaks under load)
- Using `string` interpolation in log messages (breaks structured logging)
