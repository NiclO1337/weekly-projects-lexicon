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

    public void Replace(Asset asset)
    {
        int index = assets.FindIndex(a => a.Id == asset.Id);
        if (index < 0)
        {
            throw new AssetNotFoundException(asset.Id);
        }

        assets[index] = asset;
    }

    public void Remove(int id)
    {
        int index = assets.FindIndex(a => a.Id == id);
        if (index < 0)
        {
            throw new AssetNotFoundException(id);
        }

        assets.RemoveAt(index);
    }
}
