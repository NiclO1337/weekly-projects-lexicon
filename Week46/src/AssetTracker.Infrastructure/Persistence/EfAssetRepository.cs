using AssetTracker.Core.Abstractions;
using AssetTracker.Core.Enums;
using AssetTracker.Core.Exceptions;
using AssetTracker.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace AssetTracker.Infrastructure.Persistence;

/// <summary>Stores assets in SQL Server through EF Core. Sorting and paging run in the database.</summary>
public sealed class EfAssetRepository(AppDbContext context) : IAssetRepository
{
    public PagedResult<Asset> GetPage(AssetQuery query) => DatabaseGuard.Run(() =>
    {
        var assets = context.Assets.AsNoTracking();

        var totalCount = assets.Count();
        var totalPages = Math.Max(1, (totalCount + query.PageSize - 1) / query.PageSize);

        // Clamp, so a page that no longer exists (e.g. after a removal) shows the last one instead.
        var pageNumber = Math.Clamp(query.Page, 1, totalPages);

        var items = Sort(assets, query.SortBy)
            .Include(a => a.Office)
            .Skip((pageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToList();

        return new PagedResult<Asset>(items, pageNumber, totalPages, totalCount);
    });

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

    /// <summary>
    /// Orders by <paramref name="sortBy"/>, with the ID last so assets that tie (same date) keep a
    /// fixed order and never jump between pages.
    /// </summary>
    private static IQueryable<Asset> Sort(IQueryable<Asset> assets, AssetSortMode sortBy) => sortBy switch
    {
        AssetSortMode.Office => assets
            .OrderBy(a => a.Office.Name)
            .ThenBy(a => a.PurchaseDate)
            .ThenBy(a => a.Id),
        AssetSortMode.EndOfLife => assets
            .OrderBy(a => a.PurchaseDate)
            .ThenBy(a => a.Id),
        _ => assets
            .OrderBy(a => EF.Property<AssetCategory>(a, AppDbContext.DiscriminatorColumn))
            .ThenBy(a => a.PurchaseDate)
            .ThenBy(a => a.Id),
    };
}
