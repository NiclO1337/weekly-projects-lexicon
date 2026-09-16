using AssetTracker.Models;

namespace AssetTracker.Services;

internal sealed class AssetService(IAssetRepository repository)
{
    internal void AddAsset(Asset asset)
    {
        asset.Id = repository.GetNextId();
        repository.Add(asset);
    }
}
