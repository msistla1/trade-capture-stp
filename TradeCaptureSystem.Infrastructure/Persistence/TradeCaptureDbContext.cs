using Microsoft.EntityFrameworkCore;
using TradeCaptureSystem.Domain.Entities;

namespace TradeCaptureSystem.Infrastructure.Persistence;

/// <summary>
/// EF Core DbContext for trade capture system
/// </summary>
public class TradeCaptureDbContext : DbContext
{
    public TradeCaptureDbContext(DbContextOptions<TradeCaptureDbContext> options)
        : base(options)
    {
    }

    public DbSet<Trade> Trades { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Trade>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.TradeId)
                .IsRequired()
                .HasMaxLength(50);

            entity.HasIndex(e => e.TradeId)
                .IsUnique();

            entity.Property(e => e.Counterparty)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Instrument)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.Quantity)
                .HasPrecision(18, 4);

            entity.Property(e => e.Price)
                .HasPrecision(18, 4);

            entity.Property(e => e.CurrentState)
                .HasConversion<string>();

            entity.Property(e => e.ValidationErrors)
                .HasConversion(
                    v => string.Join(";", v),
                    v => v.Split(';', StringSplitOptions.RemoveEmptyEntries).ToList()
                );
        });
    }
}
