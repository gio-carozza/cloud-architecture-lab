using System.Net;
using System.Text;
using System.Text.Json;
using Lab.Observability.Api.Services.Claude;
using Xunit;

namespace Lab.Observability.Api.Tests.Controllers;

[Collection("Integration")]
public class AiBatchControllerTests
{
    private static readonly JsonSerializerOptions _opts = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _client;
    private readonly GatewayWebApplicationFactory _factory;

    public AiBatchControllerTests(GatewayWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    // ---- Submit validation --------------------------------------------------

    [Fact]
    public async Task Submit_NullElementInList_Returns400_InvalidRequest()
    {
        using var content = new StringContent("[null]", Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/api/ai/batch", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await ReadErrorAsync(response);
        Assert.Equal("invalid_request", error.Code);
    }

    [Fact]
    public async Task GetStatus_ProviderThrowsTransientException_Returns503()
    {
        _factory.FakeBatchProvider.ExceptionToThrow = new ClaudeProviderException(
            "rate limited", isTransient: true, providerErrorCode: "rate_limit");
        try
        {
            var response = await _client.GetAsync("/api/ai/batch/any-id");
            Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        }
        finally { _factory.FakeBatchProvider.Reset(); }
    }

    [Fact]
    public async Task GetResults_ProviderThrowsTransientException_Returns503()
    {
        _factory.FakeBatchProvider.ExceptionToThrow = new ClaudeProviderException(
            "service unavailable", isTransient: true, providerErrorCode: "service_unavailable");
        try
        {
            var response = await _client.GetAsync("/api/ai/batch/any-id/results");
            Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        }
        finally { _factory.FakeBatchProvider.Reset(); }
    }

    [Fact]
    public async Task Submit_EmptyList_Returns400_InvalidRequest()
    {
        var response = await PostSubmitAsync([]);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await ReadErrorAsync(response);
        Assert.Equal("invalid_request", error.Code);
    }

    [Fact]
    public async Task Submit_NullPromptInBatch_Returns400_InvalidRequest()
    {
        var response = await PostSubmitAsync(["valid", ""]);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await ReadErrorAsync(response);
        Assert.Equal("invalid_request", error.Code);
    }

    [Fact]
    public async Task Submit_PromptExceedsMaxLength_Returns400_PromptTooLong()
    {
        var response = await PostSubmitAsync(["valid", new string('x', 32_001)]);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await ReadErrorAsync(response);
        Assert.Equal("prompt_too_long", error.Code);
    }

    [Fact]
    public async Task Submit_ExceedsMaxBatchSize_Returns400_BatchSizeExceeded()
    {
        // MaxBatchSize defaults to 100 — send 101 requests
        var prompts = Enumerable.Range(0, 101)
            .Select(i => $"Prompt {i}")
            .ToArray();

        var response = await PostSubmitAsync(prompts);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await ReadErrorAsync(response);
        Assert.Equal("batch_size_exceeded", error.Code);
    }

    // ---- Submit happy path --------------------------------------------------

    [Fact]
    public async Task Submit_ValidRequests_Returns200_WithBatchId()
    {
        var response = await PostSubmitAsync(["Hello", "World"]);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        var batchId = doc.RootElement.GetProperty("batchId").GetString();
        Assert.False(string.IsNullOrWhiteSpace(batchId));
    }

    // ---- Status + Results ---------------------------------------------------

    [Fact]
    public async Task GetStatus_ValidId_Returns200()
    {
        var response = await _client.GetAsync("/api/ai/batch/test-batch-id");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        Assert.True(doc.RootElement.TryGetProperty("batchId", out _));
        Assert.True(doc.RootElement.TryGetProperty("status", out _));
    }

    [Fact]
    public async Task GetResults_ValidId_Returns200()
    {
        var response = await _client.GetAsync("/api/ai/batch/test-batch-id/results");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        Assert.Equal(JsonValueKind.Array, doc.RootElement.ValueKind);
    }

    // ---- Helpers ------------------------------------------------------------

    private Task<HttpResponseMessage> PostSubmitAsync(IEnumerable<string> prompts)
    {
        var requests = prompts.Select(p => new { prompt = p });
        return _client.PostAsync(
            "/api/ai/batch",
            new StringContent(
                JsonSerializer.Serialize(requests),
                Encoding.UTF8,
                "application/json"));
    }

    private static async Task<ErrorBody> ReadErrorAsync(HttpResponseMessage response)
    {
        var body = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<ErrorBody>(body, _opts)!;
    }

    private sealed class ErrorBody
    {
        public string Code           { get; set; } = string.Empty;
        public string Message        { get; set; } = string.Empty;
        public string? CorrelationId { get; set; }
    }
}
