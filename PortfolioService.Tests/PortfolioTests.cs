using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using PortfolioService.Dtos;
using PortfolioService.Models;
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

            var response = await _client.GetAsync($"/api/portfolio/course/{courseId}");

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await response.Content.ReadAsStringAsync();
            
            content.Should().Be("[]");
        }

        [Fact]
        public async Task CreatePortfolioItem_ShouldSaveToDatabase()
        {
            
            var newItem = new PortfolioItem
            {
                UserId = Guid.NewGuid(),
                AssetId = Guid.NewGuid(),
                TeamId = Guid.NewGuid(),
                Quantity = 10,
                AvgPrice = 100,
                CurrentValue = 100,
                IsActive = true,
                Notes = "Test Integration",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            
            var response = await _client.PostAsJsonAsync("/api/portfolio", newItem);

            
            response.StatusCode.Should().Be(HttpStatusCode.Created);
        }
        [Fact]
        public async Task CreateAndGet_ShouldSaveAndRetrieveData()
        {
            var courseId = Guid.NewGuid();
            var assetId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var newItem = new PortfolioItem
            {
                UserId = userId,
                AssetId = assetId,
                TeamId = courseId,
                Quantity = 10,
                AvgPrice = 150.50,
                CurrentValue = 150.50,
                IsActive = true,
                Notes = "Compra Happy Path",
                
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

     
            var postResponse = await _client.PostAsJsonAsync("/api/portfolio", newItem);
            postResponse.StatusCode.Should().Be(HttpStatusCode.Created); 

           
            var getResponse = await _client.GetAsync($"/api/portfolio/course/{courseId}");

           
            getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await getResponse.Content.ReadFromJsonAsync<List<PortfolioDto>>();

            content.Should().NotBeNull();
            content.Should().HaveCount(1); 
            content.First().Quantity.Should().Be(10);
            content.First().AvgPrice.Should().Be(150.50);
            
        }
        [Fact]
        public async Task CreateWithMissingData_ShouldReturnBadRequest()
        {
           
            var invalidItem = new PortfolioItem
            {
                Quantity = 10,
               
            };

            
            var response = await _client.PostAsJsonAsync("/api/portfolio", invalidItem);

           
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
        
    }
}