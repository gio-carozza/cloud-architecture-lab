using Microsoft.AspNetCore.Mvc;
using Lab.Observability.Api.Models.AI;
using Lab.Observability.Api.Services.AI;

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
    public async Task<ActionResult> Chat(
        [FromBody] ChatRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Prompt))
        {
            return BadRequest(new { error = "Prompt is required." });
        }

        try
        {
            _logger.LogInformation("AI chat endpoint invoked.");
            var response = await _provider.SendAsync(request, cancellationToken);
            return Ok(response);
        }
        catch (ApplicationException ex)
        {
            _logger.LogError(ex, "AI provider call failed.");
            return StatusCode(502, new
            {
                error = "AI provider call failed.",
                detail = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected AI gateway error.");
            return StatusCode(500, new
            {
                error = "Unexpected server error."
            });
        }
    }
}