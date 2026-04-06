using Microsoft.AspNetCore.Mvc;
using Lab.Observability.Api.Models.AI;
using Lab.Observability.Api.Services.AI;

namespace YourNamespace.Controllers;

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
        if (string.IsNullOrWhiteSpace(request.Prompt))
        {
            return BadRequest("Prompt is required.");
        }

        _logger.LogInformation("AI chat endpoint invoked.");
        var response = await _provider.SendAsync(request, cancellationToken);
        return Ok(response);
    }
}