using AssetTracker.Exceptions;
using AssetTracker.Models;

namespace AssetTracker.Services;

internal sealed class InMemoryAssetRepository : IAssetRepository
{
    private readonly List<Asset> assets = [];

    public IReadOnlyList<Asset> GetAll()
    {
        return assets;
    }

    public int GetNextId()
    {
        return assets.Count > 0 ? assets.Max(a => a.Id) + 1 : 1;
    }

    public void Add(Asset asset)
    {
        if (assets.Any(a => a.Id == asset.Id))
        {
            throw new DuplicateAssetIdException(asset.Id);
        }

        assets.Add(asset);
    }
}
