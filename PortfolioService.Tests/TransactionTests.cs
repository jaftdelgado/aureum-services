using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PortfolioService.Data;
using PortfolioService.Dtos;
using PortfolioService.Models;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace PortfolioService.Tests
{
    [Collection("IntegrationTests")]
    public class TransactionTests
    {
        private readonly HttpClient _client;
        private readonly IntegrationTestFactory _factory;

        public TransactionTests(IntegrationTestFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task BuyAsset_ShouldCalculateWeightedAveragePrice()
        {
            var userId = Guid.NewGuid();
            var assetId = Guid.NewGuid();
            var teamId = Guid.NewGuid();

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<PortfolioContext>();
                context.PortfolioItems.Add(new PortfolioItem
                {
                    UserId = userId,
                    AssetId = assetId,
                    TeamId = teamId,
                    Quantity = 10,
                    AvgPrice = 100,
                    CurrentValue = 1000,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
                await context.SaveChangesAsync();
            }

            var transaction = new PortfolioTransactionDto
            {
                UserId = userId,
                AssetId = assetId,
                TeamId = teamId,
                Quantity = 10,
                Price = 200,
                IsBuy = true
            };

            var response = await _client.PostAsJsonAsync("/api/portfolio/transaction", transaction);

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<PortfolioContext>();
                var item = context.PortfolioItems.FirstOrDefault(p => p.UserId == userId && p.AssetId == assetId);

                item.Should().NotBeNull();
                item.Quantity.Should().Be(20);

                item.AvgPrice.Should().Be(150);
            }
        }

        [Fact]
        public async Task SellAsset_ShouldReduceQuantity_AndKeepAvgPrice()
        {
            var userId = Guid.NewGuid();
            var assetId = Guid.NewGuid();
            var teamId = Guid.NewGuid();

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<PortfolioContext>();
                context.PortfolioItems.Add(new PortfolioItem
                {
                    UserId = userId,
                    AssetId = assetId,
                    TeamId = teamId,
                    Quantity = 10,
                    AvgPrice = 100,
                    CurrentValue = 1000,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
                await context.SaveChangesAsync();
            }

            var transaction = new PortfolioTransactionDto
            {
                UserId = userId,
                AssetId = assetId,
                TeamId = teamId,
                Quantity = 3,
                Price = 120m,
                IsBuy = false
            };

            var response = await _client.PostAsJsonAsync("/api/portfolio/transaction", transaction);

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<PortfolioContext>();
                var item = context.PortfolioItems.First(p => p.UserId == userId && p.AssetId == assetId);

                item.Quantity.Should().Be(7);
                item.AvgPrice.Should().Be(100);
            }
        }
        [Fact]
        public async Task SellAsset_ShouldCalculateRealizedPnl()
        {
            var userId = Guid.NewGuid();
            var assetId = Guid.NewGuid();
            var teamId = Guid.NewGuid();

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<PortfolioContext>();
                context.PortfolioItems.Add(new PortfolioItem
                {
                    UserId = userId,
                    AssetId = assetId,
                    TeamId = teamId,
                    Quantity = 10,
                    AvgPrice = 100,
                    CurrentValue = 1000,
                    IsActive = true
                });
                await context.SaveChangesAsync();
            }

            var transaction = new PortfolioTransactionDto
            {
                UserId = userId,
                AssetId = assetId,
                TeamId = teamId,
                Quantity = 5,
                Price = 150m,
                IsBuy = false
            };

            var response = await _client.PostAsJsonAsync("/api/portfolio/transaction", transaction);
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            using (var scope = _factory.Services.CreateScope())
            {
                var marketContext = scope.ServiceProvider.GetRequiredService<MarketContext>();
                var movement = await marketContext.Movements
                                .Include(m => m.Transaction)
                                .OrderByDescending(m => m.CreatedDate)
                                .FirstOrDefaultAsync(m => m.UserId == userId && m.AssetId == assetId);

                movement.Should().NotBeNull();
                movement!.Transaction.Should().NotBeNull();
                movement.Transaction!.RealizedPnl.Should().Be(250m);
            }
        }
    }
}