using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using PortfolioService;
using PortfolioService.Data;

namespace PortfolioService.Tests
{
    public class IntegrationTestFactory : WebApplicationFactory<Program>, IAsyncLifetime
    {
        private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
            .WithImage("postgres:15")
            .WithDatabase("portfolio_test_db")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureTestServices(services =>
            {
                var descriptorPortfolio = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<PortfolioContext>));
                if (descriptorPortfolio != null) services.Remove(descriptorPortfolio);
                services.AddDbContext<PortfolioContext>(options => options.UseNpgsql(_dbContainer.GetConnectionString()));

                var descriptorMarket = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<MarketContext>));
                if (descriptorMarket != null) services.Remove(descriptorMarket);
                services.AddDbContext<MarketContext>(options => options.UseNpgsql(_dbContainer.GetConnectionString()));

                var sp = services.BuildServiceProvider();
                using (var scope = sp.CreateScope())
                {
                    var portfolioDb = scope.ServiceProvider.GetRequiredService<PortfolioContext>();
                    var marketDb = scope.ServiceProvider.GetRequiredService<MarketContext>();

                    portfolioDb.Database.OpenConnection();
                    portfolioDb.Database.ExecuteSqlRaw("CREATE EXTENSION IF NOT EXISTS \"uuid-ossp\";");
                    portfolioDb.Database.EnsureCreated();

                    marketDb.Database.ExecuteSqlRaw(@"
                        CREATE TABLE IF NOT EXISTS public.movements (
                            movementid SERIAL PRIMARY KEY,
                            publicid UUID NOT NULL,
                            userid UUID NOT NULL,
                            assetid UUID NOT NULL,
                            quantity DECIMAL NOT NULL,
                            createddate TIMESTAMP NOT NULL
                        );
                        CREATE TABLE IF NOT EXISTS public.transactions (
                            transactionid SERIAL PRIMARY KEY,
                            publicid UUID NOT NULL,
                            movementid INT NOT NULL,
                            transactionprice DECIMAL NOT NULL,
                            isbuy BOOLEAN NOT NULL,
                            realizedpnl DECIMAL NOT NULL DEFAULT 0,
                            createddate TIMESTAMP NOT NULL,
                            CONSTRAINT fk_transactions_movements FOREIGN KEY (movementid) REFERENCES public.movements(movementid)
                        );
                    ");
                }
            });
        }

        public Task InitializeAsync() => _dbContainer.StartAsync();
        public new Task DisposeAsync() => _dbContainer.StopAsync();
    }
}