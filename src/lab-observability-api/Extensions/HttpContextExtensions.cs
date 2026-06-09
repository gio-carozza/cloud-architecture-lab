using Lab.Observability.Api.Middleware;

namespace Lab.Observability.Api.Extensions;

public static class HttpContextExtensions
{
    public static string? GetCorrelationId(this HttpContext context)
    {
        if (context.Items.TryGetValue(CorrelationIdMiddleware.HeaderName, out var value))
        {
            return value?.ToString();
        }

        return null;
    }
}