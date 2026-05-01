using Lab.Observability.Api.Models.AI;
using Lab.Observability.Api.Options;
using Lab.Observability.Api.Services.Claude;
using Microsoft.Extensions.Options;

namespace Lab.Observability.Api.Services.AI;

public sealed class ClaudeChatModelProvider : IChatModelProvider
{
    private readonly ClaudeApiClient _claudeApiClient;
    private readonly ILogger<ClaudeChatModelProvider> _logger;
    private readonly AnthropicOptions _options;

    public ClaudeChatModelProvider(
        ClaudeApiClient claudeApiClient,
        IOptions<AnthropicOptions> options,
        ILogger<ClaudeChatModelProvider> logger)
    {
        _claudeApiClient = claudeApiClient;
        _logger = logger;
        _options = options.Value;
    }

    public async Task<ChatResponse> SendAsync(ChatRequest request, CancellationToken cancellationToken = default)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        if (string.IsNullOrWhiteSpace(request.Prompt))
        {
            throw new ArgumentException("Prompt is required.", nameof(request));
        }

        _logger.LogInformation(
            "Sending Claude chat request. Model={Model} PromptLength={PromptLength}",
            _options.Model,
            request.Prompt.Length);

        var payload = new
        {
            model = _options.Model,
            max_tokens = _options.MaxTokens,
            messages = new object[]
            {
                new
                {
                    role = "user",
                    content = request.Prompt
                }
            }
        };

        var responseText = await _claudeApiClient.SendChatAsync(payload, cancellationToken);

        _logger.LogInformation(
            "Claude chat request completed. Model={Model} ResponseLength={ResponseLength}",
            _options.Model,
            responseText.Length);

        return new ChatResponse
        {
            Provider = "anthropic",
            Model = _options.Model,
            Response = responseText
        };
    }
}