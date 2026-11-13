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

    // Tracked (no AsNoTracking), so the changes Asset.Update makes are what SaveChanges writes.
    public Asset? FindById(int id) => DatabaseGuard.Run(() =>
        context.Assets
            .Include(a => a.Office)
            .FirstOrDefault(a => a.Id == id));

    public bool SerialNumberExists(string serialNumber, int? exceptAssetId = null) => DatabaseGuard.Run(() =>
        context.Assets.Any(a => a.SerialNumber == serialNumber && (exceptAssetId == null || a.Id != exceptAssetId)));

    public void Add(Asset asset)
    {
        context.Assets.Add(asset);
        SaveOrForget(asset);
    }

    public void Update(Asset asset) => SaveOrForget(asset);

    public void Remove(Asset asset)
    {
        context.Assets.Remove(asset);
        SaveOrForget(asset);
    }

    /// <summary>
    /// Saves pending changes. If saving fails, the context forgets <paramref name="asset"/>, so the
    /// next save doesn't retry the failed change and the next read loads it fresh from the database.
    /// </summary>
    private void SaveOrForget(Asset asset)
    {
        try
        {
            DatabaseGuard.Run(() => context.SaveChanges());
        }
        catch (DataStoreException)
        {
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
        AssetSortMode.Category => assets
            .OrderBy(a => EF.Property<AssetCategory>(a, AppDbContext.DiscriminatorColumn))

            // ComputerAsset.Type and MobileAsset.Type share the one "Type" column, so this single
            // cast orders the types of both categories (Desktop, Laptop; Phone, Tablet).
            .ThenBy(a => ((ComputerAsset)a).Type)
            .ThenBy(a => a.PurchaseDate)
            .ThenBy(a => a.Id),
        AssetSortMode.EndOfLife => assets
            .OrderBy(a => a.PurchaseDate)
            .ThenBy(a => a.Id),
        _ => assets
            .OrderBy(a => a.Office.Name)
            .ThenBy(a => a.PurchaseDate)
            .ThenBy(a => a.Id),
    };
}
