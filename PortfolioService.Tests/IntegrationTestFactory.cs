using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.TestHost;
// TUS NAMESPACES CORRECTOS
using PortfolioService;
using PortfolioService.Data;
using System.Linq;
using System.Threading.Tasks;
using Testcontainers.PostgreSql;
using Xunit;

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
                // 1. Eliminar la configuración de DbContext de producción
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<PortfolioContext>));

                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // 2. Inyectar la conexión al contenedor de Docker
                services.AddDbContext<PortfolioContext>(options =>
                {
                    options.UseNpgsql(_dbContainer.GetConnectionString());
                });

                // 3. Preparar la Base de Datos
                var sp = services.BuildServiceProvider();
                using (var scope = sp.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<PortfolioContext>();

                    // ¡IMPORTANTE! Activamos la extensión para que funcione uuid_generate_v4()
                    db.Database.OpenConnection();
                    db.Database.ExecuteSqlRaw("CREATE EXTENSION IF NOT EXISTS \"uuid-ossp\";");

                    // Creamos las tablas
                    db.Database.EnsureCreated();
                }
            });
        }

        public Task InitializeAsync() => _dbContainer.StartAsync();

        public new Task DisposeAsync() => _dbContainer.StopAsync();
    }
}