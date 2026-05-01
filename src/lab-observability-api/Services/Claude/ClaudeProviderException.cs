using System.Net;

namespace Lab.Observability.Api.Services.Claude;

public sealed class ClaudeProviderException : Exception
{
    public string Provider { get; }
    public HttpStatusCode? ProviderStatusCode { get; }
    public string? ProviderErrorCode { get; }
    public bool IsTransient { get; }

    public ClaudeProviderException(
        string message,
        string provider = "anthropic",
        HttpStatusCode? providerStatusCode = null,
        string? providerErrorCode = null,
        bool isTransient = false,
        Exception? innerException = null)
        : base(message, innerException)
    {
        Provider = provider;
        ProviderStatusCode = providerStatusCode;
        ProviderErrorCode = providerErrorCode;
        IsTransient = isTransient;
    }
}