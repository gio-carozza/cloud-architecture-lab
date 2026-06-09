using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Xunit;

namespace Lab.Observability.Api.Tests.Controllers;

[Collection("Integration")]
public class MiddlewareTests
{
    private readonly HttpClient _client;

    public MiddlewareTests(GatewayWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task AnyRequest_SetsXCorrelationIdHeader_InResponse()
    {
        var response = await _client.GetAsync("/health");

        Assert.True(
            response.Headers.Contains("x-correlation-id"),
            "Response must include x-correlation-id header set by CorrelationIdMiddleware.");
    }

    [Fact]
    public async Task Request_WithSuppliedCorrelationId_EchoesItInResponse()
    {
        const string supplied = "test-correlation-abc123";

        using var request = new HttpRequestMessage(HttpMethod.Get, "/health");
        request.Headers.Add("x-correlation-id", supplied);

        var response = await _client.SendAsync(request);

        response.Headers.TryGetValues("x-correlation-id", out var values);
        Assert.Equal(supplied, values?.FirstOrDefault());
    }
}
