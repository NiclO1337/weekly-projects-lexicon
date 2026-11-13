using AssetTracker.Core.Exceptions;
using AssetTracker.Core.Models;

namespace AssetTracker.Core.Abstractions;

/// <summary>
/// Stores and loads assets. Every asset returned has its <see cref="Asset.Office"/> loaded.
/// Implementations throw <see cref="DataStoreException"/> when the store cannot be reached.
/// </summary>
public interface IAssetRepository
{
    /// <summary>One page of assets, sorted as <paramref name="query"/> asks.</summary>
    PagedResult<Asset> GetPage(AssetQuery query);

    /// <summary>
    /// The asset with <paramref name="id"/>, or <see langword="null"/> if there is none. Changes
    /// made to it through <see cref="Asset.Update"/> are saved by <see cref="Update"/>.
    /// </summary>
    Asset? FindById(int id);

    /// <summary>
    /// Every asset bought after <paramref name="purchasedAfter"/> and on or before
    /// <paramref name="purchasedOnOrBefore"/> (either bound may be left out), sorted as
    /// <paramref name="query"/> asks. Not paged.
    /// </summary>
    IReadOnlyList<Asset> GetAll(AssetQuery query, DateOnly? purchasedAfter = null, DateOnly? purchasedOnOrBefore = null);

    /// <summary>
    /// The <paramref name="count"/> highest-priced assets within each currency, at most
    /// <paramref name="count"/> per currency. Prices in different currencies can't be compared
    /// directly, so ranking them against each other is left to the caller, after conversion.
    /// </summary>
    IReadOnlyList<Asset> GetMostExpensivePerCurrency(int count);

    /// <summary>Asset count and summed purchase price per office and currency, grouped in the store.</summary>
    IReadOnlyList<OfficeCurrencyTotal> GetOfficeTotals();

    /// <summary>Whether any asset other than <paramref name="exceptAssetId"/> already has <paramref name="serialNumber"/>.</summary>
    bool SerialNumberExists(string serialNumber, int? exceptAssetId = null);

    /// <summary>Saves a new asset. The store assigns its <see cref="Asset.Id"/>.</summary>
    void Add(Asset asset);

    /// <summary>Saves the changes made to an asset loaded with <see cref="FindById"/>.</summary>
    void Update(Asset asset);

    /// <summary>Deletes an asset loaded with <see cref="FindById"/>.</summary>
    void Remove(Asset asset);
}
