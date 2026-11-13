using AssetTracker.Core.Enums;
using AssetTracker.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace AssetTracker.Infrastructure.Persistence;

/// <summary>
/// The EF Core session for the asset tracker database. The parameterless constructor and
/// <see cref="OnConfiguring"/> let both the app and the <c>dotnet ef</c> tools create it.
/// </summary>
public sealed class AppDbContext : DbContext
{
    // Only the localhost fallback lives in source. Set APP_DB_CONNECTION in user secrets or the
    // environment to use another server.
    private const string FallbackConnectionString =
        "Server=localhost\\SQLEXPRESS;Database=asset_tracker_ef;Integrated Security=true;Encrypt=false;";

    private const string DiscriminatorColumn = "AssetCategory";
    private const int EnumColumnLength = 20;
    private const int CurrencyCodeLength = 3;

    private static readonly IConfiguration s_configuration = new ConfigurationBuilder()
        .AddUserSecrets<AppDbContext>()
        .AddEnvironmentVariables()
        .Build();

    public DbSet<Asset> Assets => Set<Asset>();

    public DbSet<Office> Offices => Set<Office>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var connectionString = s_configuration["APP_DB_CONNECTION"] ?? FallbackConnectionString;
        optionsBuilder.UseSqlServer(connectionString);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        ConfigureOffice(modelBuilder);
        ConfigureAsset(modelBuilder);
    }

    private static void ConfigureOffice(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Office>(entity =>
        {
            entity.Property(o => o.Name).HasMaxLength(Office.MaxNameLength);
            entity.Property(o => o.Country).HasMaxLength(Office.MaxNameLength);
            entity.Property(o => o.Currency).HasConversion<string>().HasMaxLength(CurrencyCodeLength);
            entity.HasIndex(o => o.Name).IsUnique();

            // Office has no public constructor, so the seed rows are anonymous objects.
            entity.HasData(
                new { Id = 1, Name = "Germany", Country = "Germany", Currency = CurrencyCode.EUR },
                new { Id = 2, Name = "Sweden", Country = "Sweden", Currency = CurrencyCode.SEK },
                new { Id = 3, Name = "USA", Country = "USA", Currency = CurrencyCode.USD },
                new { Id = 4, Name = "Turkey", Country = "Turkey", Currency = CurrencyCode.TRY });
        });
    }

    private static void ConfigureAsset(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Asset>(entity =>
        {
            // Table-per-hierarchy: both subclasses share the Assets table, and the
            // AssetCategory column decides which class a row loads as.
            entity.HasDiscriminator<AssetCategory>(DiscriminatorColumn)
                .HasValue<ComputerAsset>(AssetCategory.Computer)
                .HasValue<MobileAsset>(AssetCategory.Mobile);
            entity.Property<AssetCategory>(DiscriminatorColumn)
                .HasConversion<string>()
                .HasMaxLength(EnumColumnLength);

            entity.Property(a => a.Brand).HasMaxLength(Asset.MaxBrandLength);
            entity.Property(a => a.Model).HasMaxLength(Asset.MaxModelLength);
            entity.Property(a => a.SerialNumber).HasMaxLength(Asset.MaxSerialNumberLength);
            entity.Property(a => a.AssignedTo).HasMaxLength(Asset.MaxAssignedToLength);
            entity.Property(a => a.PurchasePrice).HasPrecision(18, 2);
            entity.Property(a => a.Currency).HasConversion<string>().HasMaxLength(CurrencyCodeLength);
            entity.HasIndex(a => a.SerialNumber).IsUnique();

            entity.HasOne(a => a.Office)
                .WithMany()
                .HasForeignKey(a => a.OfficeId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Both subclasses have a Type property; mapping them to the same column gives one
        // readable "Type" column instead of two half-empty ones.
        modelBuilder.Entity<ComputerAsset>()
            .Property(c => c.Type)
            .HasColumnName("Type")
            .HasConversion<string>()
            .HasMaxLength(EnumColumnLength);
        modelBuilder.Entity<MobileAsset>()
            .Property(m => m.Type)
            .HasColumnName("Type")
            .HasConversion<string>()
            .HasMaxLength(EnumColumnLength);
    }
}
