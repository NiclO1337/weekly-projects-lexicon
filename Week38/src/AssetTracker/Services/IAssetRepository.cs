using AssetTracker.Models;

namespace AssetTracker.Services;

internal interface IAssetRepository
{
    IReadOnlyList<Asset> GetAll();

    int GetNextId();

    void Add(Asset asset);

    void Replace(Asset asset);

    void Remove(int id);
}
