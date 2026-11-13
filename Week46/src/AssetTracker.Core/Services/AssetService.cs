using AssetTracker.Core.Abstractions;
using AssetTracker.Core.Enums;
using AssetTracker.Core.Exceptions;
using AssetTracker.Core.Models;

namespace AssetTracker.Core.Services;

/// <summary>
/// The asset operations the UI uses. The UI hands over plain <see cref="AssetDetails"/>; this
/// class looks up the office, builds the right kind of asset and enforces the rules that need
/// the store, such as unique serial numbers.
/// </summary>
/// <param name="assets">Where assets are stored.</param>
/// <param name="offices">Where offices are loaded from.</param>
public sealed class AssetService(IAssetRepository assets, IOfficeRepository offices)
{
    /// <summary>One page of assets, sorted as <paramref name="query"/> asks.</summary>
    /// <exception cref="DataStoreException">The store could not be read.</exception>
    public PagedResult<Asset> GetPage(AssetQuery query) => assets.GetPage(query);

    /// <summary>The asset with <paramref name="id"/>.</summary>
    /// <exception cref="AssetNotFoundException">No asset has <paramref name="id"/>.</exception>
    /// <exception cref="DataStoreException">The store could not be read.</exception>
    public Asset GetById(int id) => assets.FindById(id) ?? throw new AssetNotFoundException(id);

    /// <summary>Every office, ordered by name.</summary>
    /// <exception cref="DataStoreException">The store could not be read.</exception>
    public IReadOnlyList<Office> GetOffices() => offices.GetAll();

    /// <summary>
    /// Whether an asset other than <paramref name="exceptAssetId"/> already has
    /// <paramref name="serialNumber"/> (compared trimmed, as stored). Lets the UI re-prompt
    /// instead of losing a half-filled form.
    /// </summary>
    /// <exception cref="DataStoreException">The store could not be read.</exception>
    public bool SerialNumberExists(string serialNumber, int? exceptAssetId = null) =>
        assets.SerialNumberExists(serialNumber.Trim(), exceptAssetId);

    /// <summary>Creates and saves a computer, priced in its office's currency.</summary>
    /// <exception cref="OfficeNotFoundException">No office has <see cref="AssetDetails.OfficeId"/>.</exception>
    /// <exception cref="DuplicateSerialNumberException">Another asset already has the serial number.</exception>
    /// <exception cref="ArgumentException">A text field is blank or too long.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The type, price or a date is out of range.</exception>
    /// <exception cref="DataStoreException">The store could not be read or written.</exception>
    public ComputerAsset AddComputer(ComputerType type, AssetDetails details) =>
        Add(details, office => new ComputerAsset(type, details, office));

    /// <summary>Creates and saves a phone or tablet, priced in its office's currency.</summary>
    /// <inheritdoc cref="AddComputer" path="/exception"/>
    public MobileAsset AddMobile(MobileType type, AssetDetails details) =>
        Add(details, office => new MobileAsset(type, details, office));

    /// <summary>
    /// Replaces the editable fields of the asset with <paramref name="id"/> and saves it. The
    /// price keeps the asset's own currency, even when the office changes. Nothing changes if any
    /// check fails.
    /// </summary>
    /// <exception cref="AssetNotFoundException">No asset has <paramref name="id"/>.</exception>
    /// <inheritdoc cref="AddComputer" path="/exception"/>
    public Asset Update(int id, AssetDetails details)
    {
        ArgumentNullException.ThrowIfNull(details);

        var asset = GetById(id);
        var office = FindOffice(details.OfficeId);

        // Checked before Asset.Update, so a duplicate leaves the loaded asset untouched.
        EnsureSerialNumberIsFree(details.SerialNumber.Trim(), exceptAssetId: id);

        asset.Update(details, office);
        assets.Update(asset);
        return asset;
    }

    private T Add<T>(AssetDetails details, Func<Office, T> create)
        where T : Asset
    {
        ArgumentNullException.ThrowIfNull(details);

        // Construct first: the constructor validates and trims, so the uniqueness check sees the
        // serial number exactly as it will be stored.
        var asset = create(FindOffice(details.OfficeId));
        EnsureSerialNumberIsFree(asset.SerialNumber, exceptAssetId: null);

        assets.Add(asset);
        return asset;
    }

    private Office FindOffice(int id) => offices.FindById(id) ?? throw new OfficeNotFoundException(id);

    private void EnsureSerialNumberIsFree(string serialNumber, int? exceptAssetId)
    {
        if (assets.SerialNumberExists(serialNumber, exceptAssetId))
        {
            throw new DuplicateSerialNumberException(serialNumber);
        }
    }
}
