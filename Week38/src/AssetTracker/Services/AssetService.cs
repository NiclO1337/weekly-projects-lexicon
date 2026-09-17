using AssetTracker.Models;

namespace AssetTracker.Services;

internal sealed class AssetService(IAssetRepository repository)
{
    internal const int PageSize = 5;

    internal void AddAsset(Asset asset)
    {
        asset.Id = repository.GetNextId();
        repository.Add(asset);
    }

    internal IReadOnlyList<Asset> GetSortedAssets(AssetSortMode sortMode)
    {
        IEnumerable<Asset> assets = repository.GetAll();

        return sortMode switch
        {
            AssetSortMode.AssetType => assets
                .OrderBy(a => a.GetCategoryLabel(), StringComparer.OrdinalIgnoreCase)
                .ThenBy(a => a.PurchaseDate)
                .ToList(),
            AssetSortMode.EndOfLife => assets
                .OrderBy(a => a.GetEndOfLifeDate())
                .ToList(),
            _ => assets
                .OrderBy(a => a.Office.Name, StringComparer.OrdinalIgnoreCase)
                .ThenBy(a => a.PurchaseDate)
                .ToList(),
        };
    }

    internal IReadOnlyList<Asset> SearchByBrand(string searchTerm)
    {
        return repository.GetAll()
            .Where(a => a.Brand.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    internal IReadOnlyList<Asset> SearchByModel(string searchTerm)
    {
        return repository.GetAll()
            .Where(a => a.Model.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    internal AssetsPage GetAssetsPage(AssetSortMode sortMode, int pageNumber)
    {
        IReadOnlyList<Asset> sorted = GetSortedAssets(sortMode);
        int totalPages = Math.Max(1, (int)Math.Ceiling(sorted.Count / (double)PageSize));
        int clampedPage = Math.Clamp(pageNumber, 1, totalPages);

        IReadOnlyList<Asset> pageItems = sorted
            .Skip((clampedPage - 1) * PageSize)
            .Take(PageSize)
            .ToList();

        return new AssetsPage(pageItems, clampedPage, totalPages);
    }
}
