using AssetTracker.Core.Abstractions;
using AssetTracker.Core.Exceptions;
using AssetTracker.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace AssetTracker.Infrastructure.Persistence;

/// <summary>Stores assets in SQL Server through EF Core.</summary>
public sealed class EfAssetRepository(AppDbContext context) : IAssetRepository
{
    public IReadOnlyList<Asset> GetAll() => DatabaseGuard.Run(() =>
        context.Assets
            .AsNoTracking()
            .Include(a => a.Office)
            .OrderBy(a => a.Id)
            .ToList());

    public bool SerialNumberExists(string serialNumber) => DatabaseGuard.Run(() =>
        context.Assets.Any(a => a.SerialNumber == serialNumber));

    public void Add(Asset asset)
    {
        context.Assets.Add(asset);
        try
        {
            DatabaseGuard.Run(() => context.SaveChanges());
        }
        catch (DataStoreException)
        {
            // Forget the failed insert, or the next SaveChanges would try it again.
            context.Entry(asset).State = EntityState.Detached;
            throw;
        }
    }
}
