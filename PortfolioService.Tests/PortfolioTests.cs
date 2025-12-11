using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using PortfolioService.Dtos;
using Xunit;

namespace PortfolioService.Tests
{
    [Collection("IntegrationTests")]
    public class PortfolioTests
    {
        private readonly HttpClient _client;

        public PortfolioTests(IntegrationTestFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetByCourse_ShouldReturnEmptyList_WhenNoAssetsForThatCourse()
        {
            var courseId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var response = await _client.GetAsync($"/api/portfolio/course/{courseId}?userId={userId}");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Be("[]");
        }

        [Fact]
        public async Task CreatePortfolioItem_ShouldSaveToDatabase()
        {
            var newItemDto = new
            {
                UserId = Guid.NewGuid(),
                AssetId = Guid.NewGuid(),
                TeamId = Guid.NewGuid(),
                Quantity = 10,
                AvgPrice = 100.00,
                Notes = "Test Integration"
            };

            var response = await _client.PostAsJsonAsync("/api/portfolio", newItemDto);

            response.StatusCode.Should().Be(HttpStatusCode.Created);
        }

        [Fact]
        public async Task CreateAndGet_ShouldSaveAndRetrieveData()
        {
            var courseId = Guid.NewGuid();
            var assetId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var newItemDto = new
            {
                UserId = userId,
                AssetId = assetId,
                TeamId = courseId,
                Quantity = 10,
                AvgPrice = 150.50,
                Notes = "Compra Happy Path"
            };

            var postResponse = await _client.PostAsJsonAsync("/api/portfolio", newItemDto);
            postResponse.StatusCode.Should().Be(HttpStatusCode.Created);

            var getResponse = await _client.GetAsync($"/api/portfolio/course/{courseId}?userId={userId}");

            getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await getResponse.Content.ReadFromJsonAsync<List<PortfolioDto>>();

            content.Should().NotBeNull();
            content.Should().HaveCount(1);

            var item = content!.First(); 
            item.Quantity.Should().Be(10);
            item.AvgPrice.Should().Be(150.50);
            item.AssetSymbol.Should().Be("TST");
        }

        [Fact]
        public async Task CreateWithMissingData_ShouldReturnBadRequest()
        {
            var invalidItem = new
            {
                Quantity = 10
            };

            var response = await _client.PostAsJsonAsync("/api/portfolio", invalidItem);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }
}