using AssetTracker.Models;

namespace AssetTracker.Services;

internal sealed class AssetService(IAssetRepository repository)
{
    internal void AddAsset(Asset asset)
    {
        asset.Id = repository.GetNextId();
        repository.Add(asset);
    }

    internal IReadOnlyList<Asset> GetAllAssets()
    {
        return repository.GetAll()
            .OrderBy(a => a.Office.Name, StringComparer.OrdinalIgnoreCase)
            .ThenBy(a => a.PurchaseDate)
            .ToList();
    }
}
