using Microsoft.ApplicationInsights.Extensibility;
using Lab.Observability.Api.Options;
using Lab.Observability.Api.Services.AI;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<AnthropicOptions>(
    builder.Configuration.GetSection(AnthropicOptions.SectionName));

builder.Services.AddHttpClient<IChatModelProvider, ClaudeChatModelProvider>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddApplicationInsightsTelemetry();

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/", (ILoggerFactory loggerFactory) =>
{
    var logger = loggerFactory.CreateLogger("RootEndpoint");
    logger.LogInformation("Root endpoint called at {UtcTime}", DateTime.UtcNow);

    return Results.Ok(new
    {
        service = "lab-observability-api",
        status = "running",
        environment = app.Environment.EnvironmentName,
        utcTime = DateTime.UtcNow
    });
});

app.MapGet("/health", (ILoggerFactory loggerFactory) =>
{
    var logger = loggerFactory.CreateLogger("HealthEndpoint");
    logger.LogInformation("Health endpoint called at {UtcTime}", DateTime.UtcNow);

    return Results.Ok(new
    {
        status = "healthy",
        checks = new[]
        {
            "api-process",
            "routing",
            "logging"
        },
        utcTime = DateTime.UtcNow
    });
});

app.UseAuthorization();

app.MapControllers();

app.Run();