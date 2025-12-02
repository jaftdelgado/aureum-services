using Xunit;

namespace PortfolioService.Tests
{
    [CollectionDefinition("IntegrationTests")]
    public class SharedTestContext : ICollectionFixture<IntegrationTestFactory>
    {
        
    }
}