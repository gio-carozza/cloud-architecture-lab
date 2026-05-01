using Lab.Observability.Api.Contracts;
using Lab.Observability.Api.Extensions;
using Lab.Observability.Api.Models.AI;
using Lab.Observability.Api.Services.AI;
using Microsoft.AspNetCore.Mvc;

namespace Lab.Observability.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AiController : ControllerBase
{
    private readonly IChatModelProvider _provider;
    private readonly ILogger<AiController> _logger;

    public AiController(IChatModelProvider provider, ILogger<AiController> logger)
    {
        _provider = provider;
        _logger = logger;
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

        _logger.LogInformation(
            "AI chat endpoint invoked. CorrelationId={CorrelationId} PromptLength={PromptLength}",
            HttpContext.GetCorrelationId(),
            request.Prompt.Length);

        var response = await _provider.SendAsync(request, cancellationToken);
        return Ok(response);
    }
}