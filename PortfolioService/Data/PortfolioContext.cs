using Microsoft.EntityFrameworkCore;
using PortfolioService.Models;

namespace PortfolioService.Data
{
    public class PortfolioContext : DbContext
    {
        public PortfolioContext(DbContextOptions<PortfolioContext> options) : base(options)
        {
        }

        public DbSet<PortfolioItem> PortfolioItems { get; set; }
        public DbSet<UserWallet> UserWallets { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PortfolioItem>(entity =>
            {
                entity.Property(e => e.PublicId).HasDefaultValueSql("uuid_generate_v4()").ValueGeneratedOnAdd();
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()").ValueGeneratedOnAdd();
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()").ValueGeneratedOnAdd();
            });

            modelBuilder.Entity<UserWallet>(entity =>
            {
                entity.HasKey(e => e.WalletId);

                entity.HasIndex(e => e.MembershipId)
                      .HasDatabaseName("idx_wallet_membership");

                entity.Property(e => e.PublicId)
                      .HasDefaultValueSql("uuid_generate_v4()")
                      .ValueGeneratedOnAdd();

                entity.Property(e => e.CashBalance)
                      .HasDefaultValue(0);

                entity.Property(e => e.CreatedAt)
                      .HasDefaultValueSql("now()")
                      .ValueGeneratedOnAdd();

                entity.Property(e => e.UpdatedAt)
                      .HasDefaultValueSql("now()")
                      .ValueGeneratedOnAdd();
            });
        }
    }
}