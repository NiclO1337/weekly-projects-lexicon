using AssetTracker.Core.Exceptions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace AssetTracker.Infrastructure.Persistence;

/// <summary>
/// Runs database work and turns SQL Server and EF Core failures into <see cref="DataStoreException"/>,
/// so nothing outside Infrastructure ever sees a database exception type.
/// </summary>
internal static class DatabaseGuard
{
    private const string DefaultMessage = "The database could not be read or updated.";

    internal static T Run<T>(Func<T> work, string message = DefaultMessage)
    {
        try
        {
            return work();
        }
        catch (SqlException ex)
        {
            throw new DataStoreException(message, ex);
        }
        catch (DbUpdateException ex)
        {
            throw new DataStoreException(message, ex);
        }
    }

    internal static void Run(Action work, string message = DefaultMessage) => Run(() =>
    {
        work();
        return true;
    }, message);
}
