using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using PortfolioService.Data;
using PortfolioService.Models;
using PortfolioService.Dtos; 
using Xunit;

namespace PortfolioService.Tests
{
    [Collection("IntegrationTests")]
    public class HistoryTests
    {
        private readonly HttpClient _client;
        private readonly IntegrationTestFactory _factory;

        public HistoryTests(IntegrationTestFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetHistory_ShouldReturnTransactions_WhenExists()
        {
            
            var studentId = Guid.NewGuid();
            var courseId = Guid.NewGuid(); 
            var assetId = Guid.NewGuid();

            
            using (var scope = _factory.Services.CreateScope())
            {
                var marketContext = scope.ServiceProvider.GetRequiredService<MarketContext>();

                var movement = new Movement
                {
                    PublicId = Guid.NewGuid(),
                    UserId = studentId,
                    AssetId = assetId,
                    Quantity = 10,
                    CreatedDate = DateTime.UtcNow,
                    
                    Transaction = new Transaction
                    {
                        PublicId = Guid.NewGuid(),
                        TransactionPrice = 150.00m, 
                        IsBuy = true,
                        CreatedDate = DateTime.UtcNow
                    }
                };

                marketContext.Movements.Add(movement);
                await marketContext.SaveChangesAsync();
            }

           
            var response = await _client.GetAsync($"/api/portfolio/history/course/{courseId}/student/{studentId}");

           
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var historyList = await response.Content.ReadFromJsonAsync<List<HistoryDto>>();

            historyList.Should().NotBeNull();
            historyList.Should().HaveCount(1);

            var item = historyList.First();
            item.AssetId.Should().Be(assetId);
            item.Quantity.Should().Be(10);
            item.Price.Should().Be(150.00m);
            item.Type.Should().Be("Compra"); 
            item.TotalAmount.Should().Be(1500.00m); 

           
            item.AssetName.Should().Be("Desconocido");
        }

        [Fact]
        public async Task GetHistory_ShouldReturnEmptyList_WhenNoData()
        {
            var studentId = Guid.NewGuid();
            var courseId = Guid.NewGuid();

            var response = await _client.GetAsync($"/api/portfolio/history/course/{courseId}/student/{studentId}");
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await response.Content.ReadAsStringAsync();
            content.Should().Be("[]"); 
        }
    }
}