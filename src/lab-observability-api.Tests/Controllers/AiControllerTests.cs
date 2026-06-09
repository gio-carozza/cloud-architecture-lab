using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Lab.Observability.Api.Services.Claude;
using Xunit;

namespace Lab.Observability.Api.Tests.Controllers;

[Collection("Integration")]
public class AiControllerTests
{
    private static readonly JsonSerializerOptions _opts = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly GatewayWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public AiControllerTests(GatewayWebApplicationFactory factory)
    {
        _factory = factory;
        _client  = factory.CreateClient();
    }

    // ---- Chat ---------------------------------------------------------------

    [Fact]
    public async Task Chat_EmptyPrompt_Returns400_InvalidRequest()
    {
        _factory.FakeChatProvider.Reset();

        var response = await PostChatAsync("");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await ReadErrorAsync(response);
        Assert.Equal("invalid_request", error.Code);
        Assert.NotNull(error.CorrelationId);
    }

    [Fact]
    public async Task Chat_WhitespacePrompt_Returns400_InvalidRequest()
    {
        _factory.FakeChatProvider.Reset();

        var response = await PostChatAsync("   ");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await ReadErrorAsync(response);
        Assert.Equal("invalid_request", error.Code);
    }

    [Fact]
    public async Task Chat_PromptExceedsMaxLength_Returns400_PromptTooLong()
    {
        _factory.FakeChatProvider.Reset();

        var response = await PostChatAsync(new string('x', 32_001));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await ReadErrorAsync(response);
        Assert.Equal("prompt_too_long", error.Code);
    }

    [Fact]
    public async Task Chat_ValidPrompt_Returns200()
    {
        _factory.FakeChatProvider.Reset();

        var response = await PostChatAsync("Hello");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Chat_ProviderThrowsClaudeProviderException_Returns502_SafeApiError()
    {
        _factory.FakeChatProvider.ExceptionToThrow =
            new ClaudeProviderException("upstream failed");

        var response = await PostChatAsync("Hello");

        Assert.Equal(HttpStatusCode.BadGateway, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("StackTrace", body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ClaudeProviderException", body, StringComparison.OrdinalIgnoreCase);

        var error = Deserialize<ErrorBody>(body);
        Assert.Equal("claude_provider_error", error.Code);
        Assert.NotNull(error.CorrelationId);
    }

    [Fact]
    public async Task Chat_ProviderThrowsTransientException_Returns503_SafeApiError()
    {
        _factory.FakeChatProvider.ExceptionToThrow =
            new ClaudeProviderException("rate limit", isTransient: true);

        var response = await PostChatAsync("Hello");

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        var error = Deserialize<ErrorBody>(body);
        Assert.Equal("claude_provider_error", error.Code);
    }

    // ---- StreamChat ---------------------------------------------------------

    [Fact]
    public async Task StreamChat_EmptyPrompt_Returns400_BeforeSseHeaders()
    {
        _factory.FakeChatProvider.Reset();

        var response = await PostStreamChatAsync("");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        // Content-Type must be JSON, not text/event-stream — guard fired before SSE headers
        var ct = response.Content.Headers.ContentType?.MediaType;
        Assert.NotEqual("text/event-stream", ct);
    }

    [Fact]
    public async Task StreamChat_PromptExceedsMaxLength_Returns400_BeforeSseHeaders()
    {
        _factory.FakeChatProvider.Reset();

        var response = await PostStreamChatAsync(new string('x', 32_001));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await ReadErrorAsync(response);
        Assert.Equal("prompt_too_long", error.Code);
    }

    [Fact]
    public async Task StreamChat_ValidPrompt_Returns200_TextEventStream()
    {
        _factory.FakeChatProvider.Reset();

        var response = await PostStreamChatAsync("Hello");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/event-stream",
            response.Content.Headers.ContentType?.MediaType);

        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("data:", body);
    }

    // ---- Helpers ------------------------------------------------------------

    private Task<HttpResponseMessage> PostChatAsync(string prompt) =>
        _client.PostAsync(
            "/api/ai/chat",
            new StringContent(
                JsonSerializer.Serialize(new { prompt }),
                Encoding.UTF8,
                "application/json"));

    private Task<HttpResponseMessage> PostStreamChatAsync(string prompt) =>
        _client.PostAsync(
            "/api/ai/chat/stream",
            new StringContent(
                JsonSerializer.Serialize(new { prompt }),
                Encoding.UTF8,
                "application/json"));

    private static async Task<ErrorBody> ReadErrorAsync(HttpResponseMessage response)
    {
        var body = await response.Content.ReadAsStringAsync();
        return Deserialize<ErrorBody>(body);
    }

    private static T Deserialize<T>(string json) =>
        JsonSerializer.Deserialize<T>(json, _opts)!;

    private sealed class ErrorBody
    {
        public string Code          { get; set; } = string.Empty;
        public string Message       { get; set; } = string.Empty;
        public string? CorrelationId { get; set; }
    }
}
