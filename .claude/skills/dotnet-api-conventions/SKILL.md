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
  - `Providers/` → `Lab.Observability.Api.Providers`
  - `Models/` → `Lab.Observability.Api.Models`
  - `Options/` → `Lab.Observability.Api.Options`
  - `Middleware/` → `Lab.Observability.Api.Middleware`

## Provider Abstraction (CRITICAL — do not break)

The seam is `IChatModelProvider`. ALL LLM calls go through it.

```csharp
public interface IChatModelProvider
{
    Task<ChatResponse> SendAsync(ChatRequest request, CancellationToken ct);
    string ProviderName { get; }
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

**Always:** `using Microsoft.Extensions.Options;` (the IOptions<T> gotcha).

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
[Route("api/ai")]
public class AiChatController : ControllerBase
{
    private readonly IChatModelProvider _provider;
    private readonly ILogger<AiChatController> _logger;

    public AiChatController(IChatModelProvider provider, ILogger<AiChatController> logger)
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

**Never return stack traces.** Wrap unhandled exceptions in middleware:

```csharp
// Middleware/ExceptionHandlingMiddleware.cs
public class ExceptionHandlingMiddleware
{
    public async Task InvokeAsync(HttpContext ctx, RequestDelegate next)
    {
        try { await next(ctx); }
        catch (Exception ex)
        {
            var correlationId = ctx.TraceIdentifier;
            _logger.LogError(ex, "Unhandled exception. CorrelationId={CorrelationId}", correlationId);
            ctx.Response.StatusCode = 500;
            await ctx.Response.WriteAsJsonAsync(new
            {
                error = "An unexpected error occurred.",
                correlationId
            });
        }
    }
}
```

**Provider-specific errors** (e.g., 401 from Anthropic) should be classified
and translated to safe HTTP status codes by the provider, not surfaced raw.

## Logging Conventions
- Use structured logging only: `_logger.LogInformation("Sent {Tokens} to {Provider}", tokens, name);`
- Never log secrets, API keys, or full prompt bodies in production.
- Include correlation ID in every log scope.
- Day 6 will add Serilog + Application Insights — anticipate this in code.

## Common mistakes (avoid)
- Putting Anthropic SDK types in `ChatRequest`/`ChatResponse` (breaks abstraction)
- Reading config via `IConfiguration` directly instead of `IOptions<T>` (no validation)
- Returning `Exception.Message` to clients (info disclosure)
- Forgetting `CancellationToken` (resource leaks under load)
- Using `string` interpolation in log messages (breaks structured logging)