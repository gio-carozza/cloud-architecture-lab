using Microsoft.Extensions.Primitives;
using Serilog.Context;

namespace Lab.Observability.Api.Middleware;

public sealed class CorrelationIdMiddleware
{
    public const string HeaderName = "x-correlation-id";

    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;

    public CorrelationIdMiddleware(
        RequestDelegate next,
        ILogger<CorrelationIdMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        string correlationId;

        if (context.Request.Headers.TryGetValue(HeaderName, out StringValues existing)
            && !StringValues.IsNullOrEmpty(existing))
        {
            correlationId = existing.ToString();
        }
        else
        {
            correlationId = Guid.NewGuid().ToString("N");
            context.Request.Headers[HeaderName] = correlationId;
        }

        context.Response.Headers[HeaderName] = correlationId;
        context.Items[HeaderName] = correlationId;

        // Serilog LogContext enrichment — flows into every log statement
        // emitted during this request, including from async continuations.
        using (LogContext.PushProperty("CorrelationId", correlationId))
        using (LogContext.PushProperty("Path", context.Request.Path.ToString()))
        using (LogContext.PushProperty("Method", context.Request.Method))
        // ILogger BeginScope — for non-Serilog consumers (defense in depth)
        using (_logger.BeginScope(new Dictionary<string, object?>
        {
            ["CorrelationId"] = correlationId,
            ["Path"] = context.Request.Path.ToString(),
            ["Method"] = context.Request.Method
        }))
        {
            await _next(context);
        }
    }
}