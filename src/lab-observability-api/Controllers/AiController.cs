using System.Text.Json;
using Lab.Observability.Api.Contracts;
using Lab.Observability.Api.Extensions;
using Lab.Observability.Api.Models.AI;
using Lab.Observability.Api.Options;
using Lab.Observability.Api.Services.AI;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Lab.Observability.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AiController : ControllerBase
{
    private readonly IChatModelProvider _provider;
    private readonly ILogger<AiController> _logger;
    private readonly AnthropicOptions _options;

    public AiController(
        IChatModelProvider provider,
        ILogger<AiController> logger,
        IOptions<AnthropicOptions> options)
    {
        _provider = provider;
        _logger = logger;
        _options = options.Value;
    }

    [HttpPost("chat")]
    public async Task<ActionResult<ChatResponse>> Chat(
        [FromBody] ChatRequest request,
        CancellationToken cancellationToken)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.Prompt))
        {
            return BadRequest(new ApiError(
                Code: "invalid_request",
                Message: "Prompt is required.",
                CorrelationId: HttpContext.GetCorrelationId()));
        }

        if (request.Prompt.Length > _options.MaxPromptLength)
        {
            return BadRequest(new ApiError(
                Code: "prompt_too_long",
                Message: $"Prompt exceeds the maximum allowed length of {_options.MaxPromptLength} characters.",
                CorrelationId: HttpContext.GetCorrelationId()));
        }

        _logger.LogInformation(
            "AI chat endpoint invoked. CorrelationId={CorrelationId} PromptLength={PromptLength}",
            HttpContext.GetCorrelationId(),
            request.Prompt.Length);

        var response = await _provider.SendAsync(request, cancellationToken);
        return Ok(response);
    }

    [HttpPost("chat/stream")]
    public async Task StreamChat([FromBody] ChatRequest request)
    {
        // Validate before SSE headers are written — status can still change here
        if (request is null || string.IsNullOrWhiteSpace(request.Prompt))
        {
            Response.StatusCode = StatusCodes.Status400BadRequest;
            await Response.WriteAsJsonAsync(
                new ApiError(
                    Code: "invalid_request",
                    Message: "Prompt is required.",
                    CorrelationId: HttpContext.GetCorrelationId()),
                _sseJsonOptions);
            return;
        }

        if (request.Prompt.Length > _options.MaxPromptLength)
        {
            Response.StatusCode = StatusCodes.Status400BadRequest;
            await Response.WriteAsJsonAsync(
                new ApiError(
                    Code: "prompt_too_long",
                    Message: $"Prompt exceeds the maximum allowed length of {_options.MaxPromptLength} characters.",
                    CorrelationId: HttpContext.GetCorrelationId()),
                _sseJsonOptions);
            return;
        }

        _logger.LogInformation(
            "AI chat stream endpoint invoked. CorrelationId={CorrelationId} PromptLength={PromptLength}",
            HttpContext.GetCorrelationId(),
            request.Prompt.Length);

        // Disable ASP.NET Core + nginx proxy buffering before writing any headers
        HttpContext.Features.Get<IHttpResponseBodyFeature>()?.DisableBuffering();

        Response.ContentType = "text/event-stream; charset=utf-8";
        Response.Headers.Append("Cache-Control", "no-cache");
        Response.Headers.Append("X-Accel-Buffering", "no");

        try
        {
            await foreach (var chunk in _provider.StreamAsync(request, HttpContext.RequestAborted))
            {
                await Response.WriteAsync(
                    $"data: {JsonSerializer.Serialize(chunk, _sseJsonOptions)}\n\n",
                    HttpContext.RequestAborted);
                await Response.Body.FlushAsync(HttpContext.RequestAborted);
            }
        }
        catch (OperationCanceledException) when (HttpContext.RequestAborted.IsCancellationRequested)
        {
            // Client disconnected — upstream stream already cancelled via RequestAborted
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Mid-stream error in chat/stream. CorrelationId={CorrelationId}",
                HttpContext.GetCorrelationId());

            try
            {
                var errorJson = JsonSerializer.Serialize(
                    new ApiError(
                        Code: "stream_error",
                        Message: "An error occurred during streaming.",
                        CorrelationId: HttpContext.GetCorrelationId()),
                    _sseJsonOptions);

                await Response.WriteAsync($"event: error\ndata: {errorJson}\n\n");
                await Response.Body.FlushAsync();
            }
            catch
            {
                // Suppress secondary write failures — response may already be closed
            }
        }
    }

    private static readonly JsonSerializerOptions _sseJsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
}