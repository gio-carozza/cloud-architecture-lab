using Xunit;

namespace Lab.Observability.Api.Tests;

[CollectionDefinition("Integration")]
public class IntegrationTestCollection : ICollectionFixture<GatewayWebApplicationFactory>
{
    // Marker class — all tests in [Collection("Integration")] share one factory instance.
}
