using AssetTracker.Core.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace AssetTracker.Infrastructure.Persistence;

/// <summary>Prepares the database at startup.</summary>
public static class DatabaseInitializer
{
    /// <summary>
    /// Creates the database on first run, applies any pending migrations, and inserts the sample
    /// assets if there are no assets yet.
    /// </summary>
    /// <exception cref="DataStoreException">The database server could not be reached or the database could not be prepared.</exception>
    public static void Initialize(AppDbContext context) => DatabaseGuard.Run(() =>
    {
        context.Database.Migrate();
        SeedSampleAssets(context);
    }, "Could not connect to or prepare the database.");

    private static void SeedSampleAssets(AppDbContext context)
    {
        if (context.Assets.Any())
        {
            return;
        }

        // Tracked offices, so EF links the new assets to the existing rows instead of inserting them.
        var offices = context.Offices.ToList();
        context.Assets.AddRange(SampleData.Create(offices, DateOnly.FromDateTime(DateTime.Today)));
        context.SaveChanges();

        // The context lives for the whole session; don't keep the seeded entities tracked in it.
        context.ChangeTracker.Clear();
    }
}
