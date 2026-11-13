using AssetTracker.Core.Exceptions;
using AssetTracker.Core.Models;

namespace AssetTracker.Core.Abstractions;

/// <summary>
/// Stores and loads assets. Every asset returned has its <see cref="Asset.Office"/> loaded.
/// Implementations throw <see cref="DataStoreException"/> when the store cannot be reached.
/// </summary>
public interface IAssetRepository
{
    /// <summary>Every asset, ordered by ID.</summary>
    IReadOnlyList<Asset> GetAll();

    /// <summary>Whether any asset already has <paramref name="serialNumber"/>.</summary>
    bool SerialNumberExists(string serialNumber);

    /// <summary>Saves a new asset. The store assigns its <see cref="Asset.Id"/>.</summary>
    void Add(Asset asset);
}
