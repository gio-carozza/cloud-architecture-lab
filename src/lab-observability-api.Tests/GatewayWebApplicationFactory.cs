using Lab.Observability.Api.Services.AI;
using Lab.Observability.Api.Tests.Fakes;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Lab.Observability.Api.Tests;

public class GatewayWebApplicationFactory : WebApplicationFactory<Program>
{
    public FakeChatModelProvider FakeChatProvider { get; } = new();
    public FakeBatchChatModelProvider FakeBatchProvider { get; } = new();

    // Subclasses can override to supply different config values (e.g. empty key).
    protected virtual string TestApiKey => "fake-key-for-tests";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Anthropic:ApiKey"]    = TestApiKey,
                ["Anthropic:BaseUrl"]   = "https://fake.anthropic.test/v1",
                ["Anthropic:Model"]     = "claude-sonnet-4-6",
                ["Anthropic:MaxTokens"] = "512",
            });
        });

        builder.ConfigureTestServices(services =>
        {
            var chatDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(IChatModelProvider));
            if (chatDescriptor is not null) services.Remove(chatDescriptor);

            var batchDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(IBatchChatModelProvider));
            if (batchDescriptor is not null) services.Remove(batchDescriptor);

            services.AddSingleton<IChatModelProvider>(FakeChatProvider);
            services.AddSingleton<IBatchChatModelProvider>(FakeBatchProvider);
        });
    }
}

// Used only by HealthReadyMisconfiguredTests — not a collection fixture.
public sealed class EmptyApiKeyWebApplicationFactory : GatewayWebApplicationFactory
{
    protected override string TestApiKey => "";
}
