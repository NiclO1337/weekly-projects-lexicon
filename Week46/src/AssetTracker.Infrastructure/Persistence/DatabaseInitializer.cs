using AssetTracker.Core.Exceptions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace AssetTracker.Infrastructure.Persistence;

/// <summary>Prepares the database at startup.</summary>
public static class DatabaseInitializer
{
    /// <summary>
    /// Creates the database on first run and applies any pending migrations.
    /// </summary>
    /// <exception cref="DataStoreException">The database server could not be reached or the migration failed.</exception>
    public static void Initialize(AppDbContext context)
    {
        try
        {
            context.Database.Migrate();
        }
        catch (SqlException ex)
        {
            throw new DataStoreException("Could not connect to or prepare the database.", ex);
        }
    }
}
