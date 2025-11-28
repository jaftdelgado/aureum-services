using Microsoft.EntityFrameworkCore;
using PortfolioService.Models;

namespace PortfolioService.Data
{
    public class MarketContext : DbContext
    {
        public MarketContext(DbContextOptions<MarketContext> options) : base(options)
        {
        }

        public DbSet<Movement> Movements { get; set; }
        public DbSet<Transaction> Transactions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
          
            modelBuilder.Entity<Movement>()
                .HasOne(m => m.Transaction)
                .WithOne(t => t.Movement)
                .HasForeignKey<Transaction>(t => t.MovementId);
        }
    }
}