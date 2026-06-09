using System.Text.Json;
using Xunit;

namespace Lab.Observability.Api.Tests.Controllers;

[Collection("Integration")]
public class HealthTests
{
    private readonly HttpClient _client;

    public HealthTests(GatewayWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Health_Returns200_WithHealthyStatus()
    {
        var response = await _client.GetAsync("/health");

        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);

        Assert.Equal("healthy",
            doc.RootElement.GetProperty("status").GetString());
    }

    [Fact]
    public async Task HealthLive_Returns200()
    {
        var response = await _client.GetAsync("/health/live");

        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task HealthReady_Returns200_WhenAllConfigPresent()
    {
        var response = await _client.GetAsync("/health/ready");

        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);

        Assert.Equal("ready",
            doc.RootElement.GetProperty("status").GetString());
    }
}
