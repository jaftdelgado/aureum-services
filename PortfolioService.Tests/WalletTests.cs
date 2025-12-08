using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PortfolioService.Data;
using PortfolioService.Dtos;
using PortfolioService.Models;
using Xunit;

namespace PortfolioService.Tests
{
    [Collection("IntegrationTests")]
    public class WalletTests
    {
        private readonly HttpClient _client;
        private readonly IntegrationTestFactory _factory;

        public WalletTests(IntegrationTestFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task UpdateWallet_ShouldCreateWallet_WhenNotExists()
        {
            var membershipId = Guid.NewGuid();
            var dto = new WalletDto
            {
                MembershipId = membershipId,
                CashBalance = 50000
            };
            var response = await _client.PutAsJsonAsync("/api/portfolio/wallet", dto);

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<PortfolioContext>();
                var wallet = await context.UserWallets.FirstOrDefaultAsync(w => w.MembershipId == membershipId);

                wallet.Should().NotBeNull();
                wallet!.CashBalance.Should().Be(50000);
            }
        }

        [Fact]
        public async Task UpdateWallet_ShouldUpdateBalance_WhenExists()
        {
            var membershipId = Guid.NewGuid();

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<PortfolioContext>();
                context.UserWallets.Add(new UserWallet
                {
                    MembershipId = membershipId,
                    CashBalance = 1000,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
                await context.SaveChangesAsync();
            }

            var dto = new WalletDto
            {
                MembershipId = membershipId,
                CashBalance = 75000 
            };
            var response = await _client.PutAsJsonAsync("/api/portfolio/wallet", dto);

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<PortfolioContext>();
                var wallet = await context.UserWallets.FirstAsync(w => w.MembershipId == membershipId);

                wallet.CashBalance.Should().Be(75000); 
            }
        }
    }
}