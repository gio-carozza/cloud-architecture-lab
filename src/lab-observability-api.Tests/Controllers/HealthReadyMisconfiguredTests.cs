using System.Net;
using Xunit;

namespace Lab.Observability.Api.Tests.Controllers;

// Uses its own factory (not the shared Integration collection) so it can
// supply an empty ApiKey without affecting other tests.
// DisableTestParallelization in AssemblyInfo.cs ensures this class's factory
// does not race with the Integration collection factory on startup.
public class HealthReadyMisconfiguredTests : IDisposable
{
    private readonly EmptyApiKeyWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public HealthReadyMisconfiguredTests()
    {
        _factory = new EmptyApiKeyWebApplicationFactory();
        _client  = _factory.CreateClient();
    }

    [Fact]
    public async Task HealthReady_Returns503_WhenApiKeyMissing()
    {
        var response = await _client.GetAsync("/health/ready");

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }
}
