using System.Net;
using Azure.Monitor.OpenTelemetry.AspNetCore;
using Lab.Observability.Api.Contracts;
using Lab.Observability.Api.Extensions;
using Lab.Observability.Api.Middleware;
using Lab.Observability.Api.Options;
using Lab.Observability.Api.Services.AI;
using Lab.Observability.Api.Services.Claude;
using Lab.Observability.Api.Telemetry;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Options;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Serilog;

// ---------------------------------------------------------------------------
// Bootstrap logger — captures startup errors before the host is built.
// Replaced by the host-configured Serilog after WebApplication.CreateBuilder.
// ---------------------------------------------------------------------------
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting lab-observability-api");

    var builder = WebApplication.CreateBuilder(args);

    // -----------------------------------------------------------------------
    // Serilog: replaces default ILoggerFactory.
    // Console sink for local visibility; OTel logging provider exports to AI.
    // No Serilog.Sinks.ApplicationInsights — see ADR-006.
    // -----------------------------------------------------------------------
    builder.Host.UseSerilog((context, services, config) => config
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Service", GatewayTelemetry.ServiceName)
        .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName)
        .WriteTo.Console(
            outputTemplate:
                "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} " +
                "{Properties:j}{NewLine}{Exception}"));

    // -----------------------------------------------------------------------
    // Configuration binding (preserved from Day 5)
    // -----------------------------------------------------------------------
    builder.Services.Configure<AnthropicOptions>(
        builder.Configuration.GetSection(AnthropicOptions.SectionName));

    // -----------------------------------------------------------------------
    // OpenTelemetry: the SINGLE telemetry export pipeline.
    // - UseAzureMonitor() exports traces, metrics, AND logs to App Insights.
    // - Custom ActivitySource and Meter from Day 5 are preserved.
    // -----------------------------------------------------------------------
    var appInsightsConnectionString =
        builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"];

    var openTelemetry = builder.Services.AddOpenTelemetry();

    if (!string.IsNullOrWhiteSpace(appInsightsConnectionString))
    {
        openTelemetry.UseAzureMonitor();
    }

    builder.Services.ConfigureOpenTelemetryTracerProvider((sp, otel) =>
    {
        otel.AddSource(GatewayTelemetry.ServiceName);
    });

    builder.Services.ConfigureOpenTelemetryMeterProvider((sp, otel) =>
    {
        otel.AddMeter(GatewayTelemetry.ServiceName);
    });

    builder.Services.Configure<OpenTelemetryLoggerOptions>(options =>
    {
        options.IncludeScopes = true;
        options.IncludeFormattedMessage = true;
        options.ParseStateValues = true;
    });

    // -----------------------------------------------------------------------
    // HTTP client + resilience pipeline for Claude (preserved from Day 5)
    // -----------------------------------------------------------------------
    builder.Services.AddHttpClient<ClaudeApiClient>((sp, client) =>
    {
        var options = sp.GetRequiredService<IOptions<AnthropicOptions>>().Value;

        client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
        client.DefaultRequestHeaders.Add("x-api-key", options.ApiKey);
        client.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");
        if (options.EnablePromptCaching)
            client.DefaultRequestHeaders.Add("anthropic-beta", "prompt-caching-2024-07-31");
        client.Timeout = Timeout.InfiniteTimeSpan;
    })

    .AddStandardResilienceHandler(options =>
    {
        // Per-attempt timeout — single HTTP call cannot exceed this
        options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(45);

        // Total request timeout including retries — must be >= AttemptTimeout
        options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(60);

        // Circuit breaker — open when 20% of requests in 120s window fail
        options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(120);
        options.CircuitBreaker.FailureRatio = 0.20;
        options.CircuitBreaker.MinimumThroughput = 5;
        options.CircuitBreaker.BreakDuration = TimeSpan.FromSeconds(15);

        // Retry stage present but disabled per ADR-006 — no retries on non-idempotent
        // chat POST. MaxRetryAttempts=1 + ShouldHandle=false is the final state
        // (v10 rejects MaxRetryAttempts=0 as invalid; intent is unchanged).
        options.Retry.MaxRetryAttempts = 1;
        options.Retry.ShouldHandle = _ => ValueTask.FromResult(false);
    });

    builder.Services.AddScoped<IChatModelProvider, ClaudeChatModelProvider>();

    // -----------------------------------------------------------------------
    // Batch HTTP client — separate from the interactive client.
    // NO resilience pipeline: retrying a submit may duplicate a batch job.
    // Per-call timeout is modest (each HTTP call is individually quick).
    // -----------------------------------------------------------------------
    builder.Services.AddHttpClient<ClaudeBatchApiClient>((sp, client) =>
    {
        var options = sp.GetRequiredService<IOptions<AnthropicOptions>>().Value;
        client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
        client.DefaultRequestHeaders.Add("x-api-key", options.ApiKey);
        client.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");
        client.Timeout = TimeSpan.FromSeconds(30);
    });

    builder.Services.AddScoped<IBatchChatModelProvider, ClaudeBatchChatModelProvider>();

    builder.Services.AddHealthChecks();
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    var app = builder.Build();

    // -----------------------------------------------------------------------
    // Correlation ID middleware MUST run before request logging so the
    // CorrelationId property is enriched into the per-request log scope.
    // -----------------------------------------------------------------------
    app.UseMiddleware<CorrelationIdMiddleware>();

    // -----------------------------------------------------------------------
    // Serilog request logging: one structured line per HTTP request.
    // -----------------------------------------------------------------------
    app.UseSerilogRequestLogging(opts =>
    {
        opts.MessageTemplate =
            "HTTP {RequestMethod} {RequestPath} {StatusCode} in {Elapsed:0}ms";

        opts.GetLevel = (httpContext, elapsed, ex) =>
        {
            if (ex != null) return Serilog.Events.LogEventLevel.Error;
            if (httpContext.Response.StatusCode >= 500) return Serilog.Events.LogEventLevel.Error;
            if (httpContext.Response.StatusCode >= 400) return Serilog.Events.LogEventLevel.Warning;

            // Health probes are not interesting at Information level
            var path = httpContext.Request.Path.Value ?? string.Empty;
            if (path.StartsWith("/health", StringComparison.OrdinalIgnoreCase))
                return Serilog.Events.LogEventLevel.Debug;

            return Serilog.Events.LogEventLevel.Information;
        };

        opts.EnrichDiagnosticContext = (diag, http) =>
        {
            diag.Set("CorrelationId", http.GetCorrelationId());
            diag.Set("UserAgent", http.Request.Headers.UserAgent.ToString());
            diag.Set("ClientIP", http.Connection.RemoteIpAddress?.ToString());
        };
    });

    // -----------------------------------------------------------------------
    // Global exception handling pipeline (preserved from Day 5)
    // -----------------------------------------------------------------------
    app.Use(async (context, next) =>
    {
        try
        {
            await next();
        }
        catch (ClaudeProviderException ex)
        {
            var logger = context.RequestServices
                .GetRequiredService<ILoggerFactory>()
                .CreateLogger("ClaudeProviderExceptionHandler");

            var correlationId = context.GetCorrelationId();

            logger.LogWarning(ex,
                "Claude provider failure. CorrelationId={CorrelationId} Provider={Provider} ProviderStatusCode={ProviderStatusCode} ProviderErrorCode={ProviderErrorCode} IsTransient={IsTransient}",
                correlationId,
                ex.Provider,
                ex.ProviderStatusCode,
                ex.ProviderErrorCode,
                ex.IsTransient);

            context.Response.StatusCode = ex.ProviderStatusCode switch
            {
                HttpStatusCode.TooManyRequests => StatusCodes.Status503ServiceUnavailable,
                HttpStatusCode.RequestTimeout => StatusCodes.Status504GatewayTimeout,
                _ when ex.IsTransient => StatusCodes.Status503ServiceUnavailable,
                _ => StatusCodes.Status502BadGateway
            };

            context.Response.ContentType = "application/json";

            await context.Response.WriteAsJsonAsync(new ApiError(
                Code: "claude_provider_error",
                Message: ex.IsTransient
                    ? "The AI provider is temporarily unavailable."
                    : "The AI provider request failed.",
                CorrelationId: correlationId));
        }
        catch (Exception ex)
        {
            var logger = context.RequestServices
                .GetRequiredService<ILoggerFactory>()
                .CreateLogger("GlobalExceptionHandler");

            var correlationId = context.GetCorrelationId();

            logger.LogError(ex,
                "Unhandled exception. CorrelationId={CorrelationId}",
                correlationId);

            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";

            await context.Response.WriteAsJsonAsync(new ApiError(
                Code: "internal_error",
                Message: "An unexpected error occurred.",
                CorrelationId: correlationId));
        }
    });

    app.UseSwagger();
    app.UseSwaggerUI();

    app.MapGet("/", (ILoggerFactory loggerFactory) =>
    {
        var logger = loggerFactory.CreateLogger("RootEndpoint");
        logger.LogInformation("Root endpoint called at {UtcTime}", DateTime.UtcNow);

        return Results.Ok(new
        {
            service = GatewayTelemetry.ServiceName,
            status = "running",
            environment = app.Environment.EnvironmentName,
            utcTime = DateTime.UtcNow
        });
    });

    app.MapGet("/health", (ILoggerFactory loggerFactory) =>
    {
        var logger = loggerFactory.CreateLogger("HealthEndpoint");
        logger.LogDebug("Health endpoint called at {UtcTime}", DateTime.UtcNow);

        return Results.Ok(new
        {
            status = "healthy",
            checks = new[] { "api-process", "routing", "logging" },
            utcTime = DateTime.UtcNow
        });
    });

    app.MapHealthChecks("/health/live");

    app.MapGet("/health/ready", (IOptions<AnthropicOptions> options) =>
    {
        var config = options.Value;

        var ready =
            !string.IsNullOrWhiteSpace(config.ApiKey) &&
            !string.IsNullOrWhiteSpace(config.Model) &&
            !string.IsNullOrWhiteSpace(config.BaseUrl);

        return ready
            ? Results.Ok(new { status = "ready" })
            : Results.Problem(
                "Gateway is not ready due to missing provider configuration.",
                statusCode: 503);
    });

    app.UseAuthorization();

    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "lab-observability-api terminated unexpectedly during startup");
}
finally
{
    Log.CloseAndFlush();
}

// Exposes Program to WebApplicationFactory<Program> in the test project
public partial class Program { }