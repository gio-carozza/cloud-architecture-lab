using System.Net;
using System.Text.Json;
using Lab.Observability.Api.Models.AI;
using Lab.Observability.Api.Options;
using Lab.Observability.Api.Telemetry;
using Microsoft.Extensions.Options;

namespace Lab.Observability.Api.Services.Claude;

public sealed class ClaudeBatchApiClient
{
    private readonly HttpClient _httpClient;
    private readonly AnthropicOptions _options;
    private readonly ILogger<ClaudeBatchApiClient> _logger;

    public ClaudeBatchApiClient(
        HttpClient httpClient,
        IOptions<AnthropicOptions> options,
        ILogger<ClaudeBatchApiClient> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<BatchJob> SubmitAsync(IReadOnlyList<ChatRequest> requests, CancellationToken cancellationToken)
    {
        using var activity = GatewayTelemetry.ActivitySource.StartActivity("batch.submit");
        activity?.SetTag("llm.provider", "anthropic");
        activity?.SetTag("batch.request_count", requests.Count);

        var batchRequests = requests.Select((r, i) => new
        {
            custom_id = $"request-{i}",
            @params = new
            {
                model = _options.Model,
                max_tokens = _options.MaxTokens,
                messages = new[] { new { role = "user", content = r.Prompt } }
            }
        });

        var payload = new { requests = batchRequests };
        var json = JsonSerializer.Serialize(payload);
        using var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        // NO retry — a network error on submit may have succeeded server-side.
        // Retrying would create a duplicate batch job with real billing consequences.
        using var response = await _httpClient.PostAsync("messages/batches", content, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning(
                "Batch submit failed. StatusCode={StatusCode}",
                (int)response.StatusCode);

            throw new ClaudeProviderException(
                message: $"Batch submit returned {(int)response.StatusCode}.",
                providerStatusCode: response.StatusCode,
                providerErrorCode: "batch_submit_failed",
                isTransient: response.StatusCode == HttpStatusCode.TooManyRequests ||
                             response.StatusCode == HttpStatusCode.ServiceUnavailable);
        }

        GatewayTelemetry.BatchJobsSubmitted.Add(
            1,
            new KeyValuePair<string, object?>("ai.provider", "anthropic"));

        _logger.LogInformation(
            "Batch submitted successfully. RequestCount={RequestCount}",
            requests.Count);

        return ParseBatchJob(body);
    }

    public async Task<BatchJobStatus> GetStatusAsync(string batchJobId, CancellationToken cancellationToken)
    {
        using var activity = GatewayTelemetry.ActivitySource.StartActivity("batch.poll");
        activity?.SetTag("llm.provider", "anthropic");
        activity?.SetTag("batch.job_id", batchJobId);

        using var response = await _httpClient.GetAsync(
            $"messages/batches/{batchJobId}", cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning(
                "Batch status check failed. BatchJobId={BatchJobId} StatusCode={StatusCode}",
                batchJobId, (int)response.StatusCode);

            throw new ClaudeProviderException(
                message: $"Batch status returned {(int)response.StatusCode}.",
                providerStatusCode: response.StatusCode,
                providerErrorCode: "batch_status_failed",
                isTransient: false);
        }

        return ParseBatchJobStatus(body);
    }

    public async Task<IReadOnlyList<BatchResult>> GetResultsAsync(string batchJobId, CancellationToken cancellationToken)
    {
        using var activity = GatewayTelemetry.ActivitySource.StartActivity("batch.retrieve");
        activity?.SetTag("llm.provider", "anthropic");
        activity?.SetTag("batch.job_id", batchJobId);

        using var response = await _httpClient.GetAsync(
            $"messages/batches/{batchJobId}/results", cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogWarning(
                "Batch results retrieval failed. BatchJobId={BatchJobId} StatusCode={StatusCode}",
                batchJobId, (int)response.StatusCode);

            throw new ClaudeProviderException(
                message: $"Batch results returned {(int)response.StatusCode}.",
                providerStatusCode: response.StatusCode,
                providerErrorCode: "batch_results_failed",
                isTransient: false);
        }

        var results = new List<BatchResult>();
        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream);

        string? line;
        while ((line = await reader.ReadLineAsync(cancellationToken)) is not null)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            var result = ParseBatchResult(line);
            if (result is not null) results.Add(result);
        }

        GatewayTelemetry.BatchJobsCompleted.Add(
            1,
            new KeyValuePair<string, object?>("ai.provider", "anthropic"));

        GatewayTelemetry.BatchResultCount.Record(
            results.Count,
            new KeyValuePair<string, object?>("ai.provider", "anthropic"));

        // Savings estimate: batch costs 50% of interactive rate.
        // avgInputTokens=500 per request (approximation); price=$3/1M tokens (Sonnet baseline).
        const double avgInputTokens = 500.0;
        const double inputPricePerMillionTokens = 3.0;
        var estimatedSavingsUsd = results.Count * avgInputTokens * 0.5 * (inputPricePerMillionTokens / 1_000_000.0);

        _logger.LogInformation(
            "Batch results retrieved. BatchJobId={BatchJobId} ResultCount={ResultCount} EstimatedSavingsUsd={EstimatedSavingsUsd:F6}",
            batchJobId, results.Count, estimatedSavingsUsd);

        return results;
    }

    private static BatchJob ParseBatchJob(string json)
    {
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        return new BatchJob
        {
            Id = root.TryGetProperty("id", out var id) ? id.GetString() ?? string.Empty : string.Empty,
            Status = ParseProcessingStatus(root),
            RequestCount = SumRequestCounts(root),
            CreatedAt = root.TryGetProperty("created_at", out var created) &&
                        created.TryGetDateTimeOffset(out var createdVal)
                ? createdVal
                : DateTimeOffset.UtcNow,
            ExpiresAt = root.TryGetProperty("expires_at", out var expires) &&
                        expires.TryGetDateTimeOffset(out var expiresVal)
                ? expiresVal
                : DateTimeOffset.UtcNow.AddHours(24)
        };
    }

    private static BatchJobStatus ParseBatchJobStatus(string json)
    {
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        var status = new BatchJobStatus
        {
            Id = root.TryGetProperty("id", out var id) ? id.GetString() ?? string.Empty : string.Empty,
            Status = ParseProcessingStatus(root),
            RequestCount = SumRequestCounts(root)
        };

        if (root.TryGetProperty("request_counts", out var counts) &&
            counts.ValueKind == JsonValueKind.Object)
        {
            if (counts.TryGetProperty("succeeded", out var s) && s.TryGetInt32(out var sv))
                status.SucceededCount = sv;
            if (counts.TryGetProperty("errored", out var e) && e.TryGetInt32(out var ev))
                status.ErroredCount = ev;
            if (counts.TryGetProperty("canceled", out var c) && c.TryGetInt32(out var cv))
                status.CanceledCount = cv;
            if (counts.TryGetProperty("expired", out var x) && x.TryGetInt32(out var xv))
                status.ExpiredCount = xv;
        }

        return status;
    }

    private static BatchResult? ParseBatchResult(string jsonLine)
    {
        try
        {
            using var doc = JsonDocument.Parse(jsonLine);
            var root = doc.RootElement;

            var customId = root.TryGetProperty("custom_id", out var cid)
                ? cid.GetString() ?? string.Empty
                : string.Empty;

            if (!root.TryGetProperty("result", out var result)) return null;

            var resultType = result.TryGetProperty("type", out var t) ? t.GetString() : null;
            var isSuccess = resultType == "succeeded";

            ChatResponse? chatResponse = null;
            string? errorMessage = null;

            if (isSuccess && result.TryGetProperty("message", out var message))
            {
                var text = TryExtractText(message);
                var model = message.TryGetProperty("model", out var m)
                    ? m.GetString() ?? string.Empty
                    : string.Empty;

                chatResponse = new ChatResponse
                {
                    Provider = "anthropic",
                    Model = model,
                    Response = text ?? string.Empty
                };
            }
            else if (!isSuccess && result.TryGetProperty("error", out var error))
            {
                errorMessage = error.TryGetProperty("message", out var em)
                    ? em.GetString()
                    : resultType;
            }

            return new BatchResult
            {
                CustomId = customId,
                IsSuccess = isSuccess,
                Response = chatResponse,
                ErrorMessage = errorMessage
            };
        }
        catch
        {
            return null;
        }
    }

    private static BatchProcessingStatus ParseProcessingStatus(JsonElement root)
    {
        if (!root.TryGetProperty("processing_status", out var statusEl))
            return BatchProcessingStatus.InProgress;

        return statusEl.GetString() switch
        {
            "canceling" => BatchProcessingStatus.Canceling,
            "ended"     => BatchProcessingStatus.Ended,
            _           => BatchProcessingStatus.InProgress
        };
    }

    private static int SumRequestCounts(JsonElement root)
    {
        if (!root.TryGetProperty("request_counts", out var counts) ||
            counts.ValueKind != JsonValueKind.Object)
            return 0;

        int total = 0;
        foreach (var prop in counts.EnumerateObject())
        {
            if (prop.Value.TryGetInt32(out var v)) total += v;
        }
        return total;
    }

    private static string? TryExtractText(JsonElement message)
    {
        if (!message.TryGetProperty("content", out var content) ||
            content.ValueKind != JsonValueKind.Array)
            return null;

        foreach (var item in content.EnumerateArray())
        {
            if (item.TryGetProperty("type", out var type) &&
                type.GetString() == "text" &&
                item.TryGetProperty("text", out var text))
            {
                return text.GetString();
            }
        }

        return null;
    }
}
