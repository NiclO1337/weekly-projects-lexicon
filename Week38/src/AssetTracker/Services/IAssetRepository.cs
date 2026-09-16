using AssetTracker.Models;

namespace AssetTracker.Services;

internal interface IAssetRepository
{
    IReadOnlyList<Asset> GetAll();

    int GetNextId();

    void Add(Asset asset);
}
