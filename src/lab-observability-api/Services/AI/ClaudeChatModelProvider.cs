using System.Diagnostics;
using System.Runtime.CompilerServices;
using Lab.Observability.Api.Models.AI;
using Lab.Observability.Api.Options;
using Lab.Observability.Api.Services.Claude;
using Lab.Observability.Api.Telemetry;
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

        using var activity = GatewayTelemetry.ActivitySource.StartActivity("ai.chat.complete");
        activity?.SetTag("llm.provider", "anthropic");
        activity?.SetTag("llm.model", _options.Model);

        _logger.LogInformation(
            "Sending Claude chat request. Model={Model} PromptLength={PromptLength}",
            _options.Model,
            request.Prompt.Length);

        try
        {
            var payload = new
            {
                model = _options.Model,
                max_tokens = _options.MaxTokens,
                messages = new object[]
                {
                    new { role = "user", content = request.Prompt }
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
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw;
        }
    }

    public async IAsyncEnumerable<ChatChunk> StreamAsync(
        ChatRequest request,
        [EnumeratorCancellation] CancellationToken ct)
    {
        if (request is null) throw new ArgumentNullException(nameof(request));
        if (string.IsNullOrWhiteSpace(request.Prompt))
            throw new ArgumentException("Prompt is required.", nameof(request));

        using var activity = GatewayTelemetry.ActivitySource.StartActivity("ai.chat.stream");
        activity?.SetTag("llm.provider", "anthropic");
        activity?.SetTag("llm.model", _options.Model);

        _logger.LogInformation(
            "Sending Claude streaming chat request. Model={Model} PromptLength={PromptLength}",
            _options.Model,
            request.Prompt.Length);

        var payload = new
        {
            model = _options.Model,
            max_tokens = _options.MaxTokens,
            stream = true,
            messages = new object[]
            {
                new { role = "user", content = request.Prompt }
            }
        };

        var ttftStopwatch = Stopwatch.StartNew();
        bool firstChunk = true;
        bool usageLogged = false;

        // try/finally (no catch) is allowed in async iterators — CS1626 only blocks yield
        // inside try blocks that have a catch clause. The finally here ensures the audit
        // trail is closed even when the client disconnects (OperationCanceledException)
        // before message_delta arrives.
        try
        {
            await foreach (var chunk in _claudeApiClient.StreamChatAsync(payload, ct))
            {
                if (firstChunk)
                {
                    GatewayTelemetry.StreamTtftMs.Record(
                        ttftStopwatch.Elapsed.TotalMilliseconds,
                        new KeyValuePair<string, object?>("ai.provider", "anthropic"),
                        new KeyValuePair<string, object?>("ai.model", _options.Model));

                    _logger.LogInformation(
                        "Claude streaming first token received. Model={Model} TtftMs={TtftMs}",
                        _options.Model,
                        ttftStopwatch.Elapsed.TotalMilliseconds);

                    firstChunk = false;
                }

                if (chunk.Usage is not null)
                {
                    _logger.LogInformation(
                        "Claude streaming completed. Model={Model} InputTokens={InputTokens} OutputTokens={OutputTokens} CacheReadTokens={CacheReadTokens}",
                        _options.Model,
                        chunk.Usage.InputTokens,
                        chunk.Usage.OutputTokens,
                        chunk.Usage.CacheReadTokens);

                    usageLogged = true;
                }

                yield return chunk;
            }
        }
        finally
        {
            if (!usageLogged)
            {
                _logger.LogWarning(
                    "Streaming session ended before final usage data was received (client disconnect or mid-stream error). Model={Model} DurationMs={DurationMs}",
                    _options.Model,
                    ttftStopwatch.Elapsed.TotalMilliseconds);
            }
        }
    }
}