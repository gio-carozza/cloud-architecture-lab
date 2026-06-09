namespace Lab.Observability.Api.Contracts;

public sealed record ApiError(
    string Code,
    string Message,
    string? CorrelationId = null);