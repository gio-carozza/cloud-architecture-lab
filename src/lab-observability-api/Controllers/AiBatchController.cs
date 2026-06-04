using Lab.Observability.Api.Contracts;
using Lab.Observability.Api.Extensions;
using Lab.Observability.Api.Models.AI;
using Lab.Observability.Api.Services.AI;
using Microsoft.AspNetCore.Mvc;

namespace Lab.Observability.Api.Controllers;

[ApiController]
[Route("api/ai/batch")]
public class AiBatchController : ControllerBase
{
    private readonly IBatchChatModelProvider _provider;
    private readonly ILogger<AiBatchController> _logger;

    public AiBatchController(IBatchChatModelProvider provider, ILogger<AiBatchController> logger)
    {
        _provider = provider;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Submit(
        [FromBody] IReadOnlyList<ChatRequest> requests,
        CancellationToken cancellationToken)
    {
        if (requests is null || requests.Count == 0)
        {
            return BadRequest(new ApiError(
                Code: "invalid_request",
                Message: "At least one request is required.",
                CorrelationId: HttpContext.GetCorrelationId()));
        }

        if (requests.Any(r => string.IsNullOrWhiteSpace(r.Prompt)))
        {
            return BadRequest(new ApiError(
                Code: "invalid_request",
                Message: "All requests must have a non-empty prompt.",
                CorrelationId: HttpContext.GetCorrelationId()));
        }

        _logger.LogInformation(
            "Batch submit invoked. CorrelationId={CorrelationId} RequestCount={RequestCount}",
            HttpContext.GetCorrelationId(),
            requests.Count);

        var job = await _provider.SubmitBatchAsync(requests, cancellationToken);

        return Ok(new
        {
            batchId = job.Id,
            submittedAt = job.CreatedAt,
            requestCount = job.RequestCount
        });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetStatus(
        string id,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Batch status check. CorrelationId={CorrelationId} BatchJobId={BatchJobId}",
            HttpContext.GetCorrelationId(),
            id);

        var status = await _provider.GetBatchStatusAsync(id, cancellationToken);

        return Ok(new
        {
            batchId = status.Id,
            status = status.Status.ToString(),
            requestCount = status.RequestCount,
            completedCount = status.SucceededCount + status.ErroredCount +
                             status.CanceledCount + status.ExpiredCount
        });
    }

    [HttpGet("{id}/results")]
    public async Task<IActionResult> GetResults(
        string id,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Batch results retrieval. CorrelationId={CorrelationId} BatchJobId={BatchJobId}",
            HttpContext.GetCorrelationId(),
            id);

        var results = await _provider.GetBatchResultsAsync(id, cancellationToken);

        return Ok(results);
    }
}
