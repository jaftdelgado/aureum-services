using Microsoft.EntityFrameworkCore;
using TeamService.Models;
using TeamService.Models.Enums;

namespace TeamService.Data
{
    public class TeamDbContext : DbContext
    {
        public TeamDbContext(DbContextOptions<TeamDbContext> options)
            : base(options)
        {
        }

        public DbSet<Team> Teams { get; set; }
        public DbSet<TeamMembership> TeamMemberships { get; set; }
        public DbSet<TeamAsset> TeamAssets { get; set; }
        public DbSet<MarketConfiguration> MarketConfigurations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.HasPostgresEnum<CurrencyEnum>("currency_enum");
            modelBuilder.HasPostgresEnum<VolatilityEnum>("volatility_enum");
            modelBuilder.HasPostgresEnum<ThickSpeedEnum>("thick_speed_enum");
            modelBuilder.HasPostgresEnum<TransactionFeeEnum>("transaction_fee_enum");

            modelBuilder.Entity<Team>()
                .HasMany(t => t.TeamMemberships)
                .WithOne(m => m.Team)
                .HasForeignKey(m => m.TeamId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Team>()
                .HasMany(t => t.TeamAssets)
                .WithOne(a => a.Team)
                .HasForeignKey(a => a.TeamId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Team>()
                .HasOne(t => t.MarketConfiguration)
                .WithOne(mc => mc.Team)
                .HasForeignKey<MarketConfiguration>(mc => mc.TeamId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Team>(entity =>
            {
                entity.ToTable("team", "public");
                entity.HasKey(e => e.TeamId);
                entity.Property(e => e.TeamName).HasMaxLength(48).IsRequired();
                entity.Property(e => e.Description).HasMaxLength(128);
                entity.Property(e => e.TeamPic).HasMaxLength(255);
            });

            modelBuilder.Entity<TeamMembership>(entity =>
            {
                entity.ToTable("teammembership", "public");
                entity.HasKey(e => e.MembershipId);
                entity.HasIndex(e => e.TeamId);
                entity.HasIndex(e => e.UserId);
            });

            modelBuilder.Entity<TeamAsset>(entity =>
            {
                entity.ToTable("teamasset", "public");
                entity.HasKey(e => e.TeamAssetId);
                entity.HasIndex(e => e.TeamId);
                entity.HasIndex(e => e.AssetId);
            });

            modelBuilder.Entity<MarketConfiguration>(entity =>
            {
                entity.ToTable("marketconfiguration", "public");
                entity.HasKey(e => e.ConfigId);
                entity.HasIndex(e => e.TeamId).IsUnique();
            });
        }
    }
}
