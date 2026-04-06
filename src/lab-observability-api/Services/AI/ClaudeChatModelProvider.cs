using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Lab.Observability.Api.Models.AI;
using Lab.Observability.Api.Options;

namespace Lab.Observability.Api.Services.AI;

public class ClaudeChatModelProvider : IChatModelProvider
{
    private readonly HttpClient _httpClient;
    private readonly AnthropicOptions _options;
    private readonly ILogger<ClaudeChatModelProvider> _logger;

    public ClaudeChatModelProvider(
        HttpClient httpClient,
        IOptions<AnthropicOptions> options,
        ILogger<ClaudeChatModelProvider> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<ChatResponse> SendAsync(ChatRequest request, CancellationToken cancellationToken = default)
    {
        var endpoint = $"{_options.BaseUrl.TrimEnd('/')}/v1/messages";

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, endpoint);

        httpRequest.Headers.Add("x-api-key", _options.ApiKey);
        httpRequest.Headers.Add("anthropic-version", "2023-06-01");
        httpRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var payload = new
        {
            model = _options.Model,
            max_tokens = _options.MaxTokens,
            messages = new[]
            {
                new
                {
                    role = "user",
                    content = request.Prompt
                }
            }
        };

        var json = JsonSerializer.Serialize(payload);
        httpRequest.Content = new StringContent(json, Encoding.UTF8, "application/json");

        _logger.LogInformation("Sending prompt to Claude model {Model}", _options.Model);

        using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Claude call failed. Status: {StatusCode}. Body: {Body}",
                response.StatusCode, responseBody);

            throw new ApplicationException($"Claude API call failed with status {(int)response.StatusCode}");
        }

        using var document = JsonDocument.Parse(responseBody);

        var outputText = document.RootElement
            .GetProperty("content")[0]
            .GetProperty("text")
            .GetString() ?? string.Empty;

        return new ChatResponse
        {
            Provider = "Anthropic",
            Model = _options.Model,
            Output = outputText
        };
    }
}