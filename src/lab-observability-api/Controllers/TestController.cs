using Microsoft.AspNetCore.Mvc;

namespace Lab.Observability.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    private readonly ILogger<TestController> _logger;

    public TestController(ILogger<TestController> logger)
    {
        _logger = logger;
    }

    [HttpGet("ping")]
    public IActionResult Ping()
    {
        _logger.LogInformation("Ping endpoint executed at {UtcTime}", DateTime.UtcNow);

        return Ok(new
        {
            message = "pong",
            service = "lab-observability-api",
            machine = Environment.MachineName,
            utcTime = DateTime.UtcNow
        });
    }

    [HttpGet("error")]
    public IActionResult Error()
    {
        _logger.LogWarning("Intentional error endpoint triggered at {UtcTime}", DateTime.UtcNow);

        throw new InvalidOperationException("Intentional test exception for Application Insights validation.");
    }
}