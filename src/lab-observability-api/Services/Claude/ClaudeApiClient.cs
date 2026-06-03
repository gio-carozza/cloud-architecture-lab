using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Lab.Observability.Api.Options;
using Lab.Observability.Api.Telemetry;
using Microsoft.Extensions.Options;
using Polly.CircuitBreaker;
using Polly.Timeout;

namespace Lab.Observability.Api.Services.Claude;

public sealed class ClaudeApiClient
{
    private readonly HttpClient _httpClient;
    private readonly AnthropicOptions _options;
    private readonly ILogger<ClaudeApiClient> _logger;

    public ClaudeApiClient(
        HttpClient httpClient,
        IOptions<AnthropicOptions> options,
        ILogger<ClaudeApiClient> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<string> SendChatAsync(object payload, CancellationToken cancellationToken)
    {
        using var activity = GatewayTelemetry.ActivitySource.StartActivity("claude.chat.api");
        activity?.SetTag("llm.provider", "anthropic");
        activity?.SetTag("llm.model", _options.Model);
        activity?.SetTag("llm.endpoint", _options.BaseUrl.TrimEnd('/') + "/messages");

        var stopwatch = Stopwatch.StartNew();

        GatewayTelemetry.ProviderRequestCount.Add(
            1,
            new KeyValuePair<string, object?>("ai.provider", "anthropic"),
            new KeyValuePair<string, object?>("ai.model", _options.Model));

        try
        {
            var anthropicRequest = BuildAnthropicRequest(payload);
            using var response = await _httpClient.PostAsJsonAsync(
                "messages",
                anthropicRequest,
                cancellationToken);

            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            activity?.SetTag("http.status_code", (int)response.StatusCode);

            if (!response.IsSuccessStatusCode)
            {
                GatewayTelemetry.ProviderFailureCount.Add(
                    1,
                    new KeyValuePair<string, object?>("ai.provider", "anthropic"),
                    new KeyValuePair<string, object?>("ai.model", _options.Model),
                    new KeyValuePair<string, object?>("http.status_code", (int)response.StatusCode));

                var providerErrorMessage = TryExtractErrorMessage(responseBody);
                var providerErrorCode = TryExtractErrorCode(responseBody);

                var isTransient =
                    response.StatusCode == HttpStatusCode.TooManyRequests ||
                    response.StatusCode == HttpStatusCode.RequestTimeout ||
                    response.StatusCode == HttpStatusCode.BadGateway ||
                    response.StatusCode == HttpStatusCode.ServiceUnavailable ||
                    response.StatusCode == HttpStatusCode.GatewayTimeout;

                _logger.LogWarning(
                    "Claude provider returned non-success status. StatusCode={StatusCode} ProviderErrorCode={ProviderErrorCode} IsTransient={IsTransient}",
                    (int)response.StatusCode,
                    providerErrorCode,
                    isTransient);

                activity?.SetStatus(ActivityStatusCode.Error,
                    providerErrorMessage ?? "Provider returned non-success status");
                activity?.SetTag("llm.latency_ms", stopwatch.Elapsed.TotalMilliseconds);

                throw new ClaudeProviderException(
                    message: string.IsNullOrWhiteSpace(providerErrorMessage)
                        ? $"Claude provider returned {(int)response.StatusCode}."
                        : providerErrorMessage,
                    providerStatusCode: response.StatusCode,
                    providerErrorCode: providerErrorCode,
                    isTransient: isTransient);
            }

            var responseText = TryExtractText(responseBody);

            var (inputTokens, outputTokens, cacheReadTokens, cacheCreationTokens) = TryExtractUsage(responseBody);
            if (inputTokens.HasValue) activity?.SetTag("llm.tokens.input", inputTokens.Value);
            if (outputTokens.HasValue) activity?.SetTag("llm.tokens.output", outputTokens.Value);

            if (cacheReadTokens.HasValue && cacheReadTokens.Value > 0)
            {
                activity?.SetTag("llm.cache.read_tokens", cacheReadTokens.Value);
                GatewayTelemetry.CacheHits.Add(
                    1,
                    new KeyValuePair<string, object?>("ai.provider", "anthropic"),
                    new KeyValuePair<string, object?>("ai.model", _options.Model));
            }

            if (cacheCreationTokens.HasValue && cacheCreationTokens.Value > 0)
            {
                activity?.SetTag("llm.cache.creation_tokens", cacheCreationTokens.Value);
                GatewayTelemetry.CacheMisses.Add(
                    1,
                    new KeyValuePair<string, object?>("ai.provider", "anthropic"),
                    new KeyValuePair<string, object?>("ai.model", _options.Model));
            }

            if ((cacheReadTokens ?? 0) > 0 || (cacheCreationTokens ?? 0) > 0)
            {
                _logger.LogInformation(
                    "Prompt cache activity. CacheReadTokens={CacheReadTokens} CacheCreationTokens={CacheCreationTokens}",
                    cacheReadTokens ?? 0,
                    cacheCreationTokens ?? 0);
            }

            activity?.SetTag("llm.latency_ms", stopwatch.Elapsed.TotalMilliseconds);

            GatewayTelemetry.ProviderLatencyMs.Record(
                stopwatch.Elapsed.TotalMilliseconds,
                new KeyValuePair<string, object?>("ai.provider", "anthropic"),
                new KeyValuePair<string, object?>("ai.model", _options.Model),
                new KeyValuePair<string, object?>("http.status_code", (int)response.StatusCode));

            _logger.LogInformation(
                "Claude provider request completed. StatusCode={StatusCode} DurationMs={DurationMs}",
                (int)response.StatusCode,
                stopwatch.Elapsed.TotalMilliseconds);

            return string.IsNullOrWhiteSpace(responseText)
                ? responseBody
                : responseText;
        }
        catch (BrokenCircuitException ex)
        {
            GatewayTelemetry.ProviderFailureCount.Add(
                1,
                new KeyValuePair<string, object?>("ai.provider", "anthropic"),
                new KeyValuePair<string, object?>("ai.model", _options.Model));

            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.SetTag("llm.latency_ms", stopwatch.Elapsed.TotalMilliseconds);

            throw new ClaudeProviderException(
                message: "Claude provider circuit is open due to recent transient failures.",
                providerStatusCode: HttpStatusCode.ServiceUnavailable,
                providerErrorCode: "circuit_open",
                isTransient: true,
                innerException: ex);
        }
        catch (TimeoutRejectedException ex)
        {
            GatewayTelemetry.ProviderFailureCount.Add(
                1,
                new KeyValuePair<string, object?>("ai.provider", "anthropic"),
                new KeyValuePair<string, object?>("ai.model", _options.Model));

            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.SetTag("llm.latency_ms", stopwatch.Elapsed.TotalMilliseconds);

            throw new ClaudeProviderException(
                message: "Claude provider request exceeded the configured timeout.",
                providerStatusCode: HttpStatusCode.RequestTimeout,
                providerErrorCode: "timeout_rejected",
                isTransient: true,
                innerException: ex);
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            GatewayTelemetry.ProviderFailureCount.Add(
                1,
                new KeyValuePair<string, object?>("ai.provider", "anthropic"),
                new KeyValuePair<string, object?>("ai.model", _options.Model));

            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.SetTag("llm.latency_ms", stopwatch.Elapsed.TotalMilliseconds);

            throw new ClaudeProviderException(
                message: "Claude provider request timed out.",
                providerStatusCode: HttpStatusCode.RequestTimeout,
                providerErrorCode: "task_canceled_timeout",
                isTransient: true,
                innerException: ex);
        }
        catch (HttpRequestException ex)
        {
            GatewayTelemetry.ProviderFailureCount.Add(
                1,
                new KeyValuePair<string, object?>("ai.provider", "anthropic"),
                new KeyValuePair<string, object?>("ai.model", _options.Model));

            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.SetTag("llm.latency_ms", stopwatch.Elapsed.TotalMilliseconds);

            throw new ClaudeProviderException(
                message: "Claude provider request failed at the network level.",
                providerErrorCode: "network_error",
                isTransient: true,
                innerException: ex);
        }
    }

    private static string? TryExtractText(string responseBody)
    {
        try
        {
            using var document = JsonDocument.Parse(responseBody);

            if (!document.RootElement.TryGetProperty("content", out var contentElement) ||
                contentElement.ValueKind != JsonValueKind.Array)
            {
                return null;
            }

            var builder = new StringBuilder();

            foreach (var item in contentElement.EnumerateArray())
            {
                if (item.ValueKind != JsonValueKind.Object)
                {
                    continue;
                }

                if (item.TryGetProperty("type", out var typeElement) &&
                    typeElement.GetString() == "text" &&
                    item.TryGetProperty("text", out var textElement))
                {
                    var text = textElement.GetString();

                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        if (builder.Length > 0)
                        {
                            builder.AppendLine();
                        }

                        builder.Append(text);
                    }
                }
            }

            return builder.Length == 0 ? null : builder.ToString();
        }
        catch
        {
            return null;
        }
    }

    private static string? TryExtractErrorMessage(string responseBody)
    {
        try
        {
            using var document = JsonDocument.Parse(responseBody);

            if (document.RootElement.TryGetProperty("error", out var errorElement) &&
                errorElement.ValueKind == JsonValueKind.Object &&
                errorElement.TryGetProperty("message", out var messageElement))
            {
                return messageElement.GetString();
            }

            if (document.RootElement.TryGetProperty("message", out var rootMessageElement))
            {
                return rootMessageElement.GetString();
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    private static string? TryExtractErrorCode(string responseBody)
    {
        try
        {
            using var document = JsonDocument.Parse(responseBody);

            if (document.RootElement.TryGetProperty("error", out var errorElement) &&
                errorElement.ValueKind == JsonValueKind.Object &&
                errorElement.TryGetProperty("type", out var typeElement))
            {
                return typeElement.GetString();
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    private static (int? InputTokens, int? OutputTokens, int? CacheReadTokens, int? CacheCreationTokens) TryExtractUsage(string responseBody)
    {
        try
        {
            using var document = JsonDocument.Parse(responseBody);

            if (!document.RootElement.TryGetProperty("usage", out var usageElement) ||
                usageElement.ValueKind != JsonValueKind.Object)
            {
                return (null, null, null, null);
            }

            int? inputTokens = null;
            int? outputTokens = null;
            int? cacheReadTokens = null;
            int? cacheCreationTokens = null;

            if (usageElement.TryGetProperty("input_tokens", out var inputEl) &&
                inputEl.TryGetInt32(out var inputVal))
            {
                inputTokens = inputVal;
            }

            if (usageElement.TryGetProperty("output_tokens", out var outputEl) &&
                outputEl.TryGetInt32(out var outputVal))
            {
                outputTokens = outputVal;
            }

            if (usageElement.TryGetProperty("cache_read_input_tokens", out var cacheReadEl) &&
                cacheReadEl.TryGetInt32(out var cacheReadVal))
            {
                cacheReadTokens = cacheReadVal;
            }

            if (usageElement.TryGetProperty("cache_creation_input_tokens", out var cacheCreationEl) &&
                cacheCreationEl.TryGetInt32(out var cacheCreationVal) &&
                cacheCreationVal > 0)
            {
                cacheCreationTokens = cacheCreationVal;
            }

            // Newer API format: cache_creation.ephemeral_1h_input_tokens / ephemeral_5m_input_tokens
            if (cacheCreationTokens is null or 0 &&
                usageElement.TryGetProperty("cache_creation", out var cacheCreationObj) &&
                cacheCreationObj.ValueKind == JsonValueKind.Object)
            {
                int sum = 0;
                if (cacheCreationObj.TryGetProperty("ephemeral_1h_input_tokens", out var h1El) &&
                    h1El.TryGetInt32(out var h1Val)) sum += h1Val;
                if (cacheCreationObj.TryGetProperty("ephemeral_5m_input_tokens", out var m5El) &&
                    m5El.TryGetInt32(out var m5Val)) sum += m5Val;
                if (sum > 0) cacheCreationTokens = sum;
            }

            return (inputTokens, outputTokens, cacheReadTokens, cacheCreationTokens);
        }
        catch
        {
            return (null, null, null, null);
        }
    }

    private JsonNode BuildAnthropicRequest(object basePayload)
    {
        var node = JsonNode.Parse(JsonSerializer.Serialize(basePayload))!.AsObject();

        if (string.IsNullOrEmpty(_options.SystemPrompt))
        {
            _logger.LogDebug("BuildAnthropicRequest: SystemPrompt empty — no system field added");
            return node;
        }

        if (!_options.EnablePromptCaching)
        {
            node["system"] = _options.SystemPrompt;
            _logger.LogDebug("BuildAnthropicRequest: system added as plain string (caching disabled)");
        }
        else
        {
            node["system"] = new JsonArray
            {
                new JsonObject
                {
                    ["type"] = "text",
                    ["text"] = _options.SystemPrompt,
                    ["cache_control"] = new JsonObject { ["type"] = "ephemeral", ["ttl"] = "1h" }
                }
            };
            _logger.LogDebug(
                "BuildAnthropicRequest: system added as content array with cache_control (SystemPromptLength={SystemPromptLength})",
                _options.SystemPrompt.Length);
        }

        return node;
    }
}