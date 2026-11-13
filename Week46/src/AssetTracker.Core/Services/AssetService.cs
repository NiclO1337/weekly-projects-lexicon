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
    /// <summary>Every asset, ordered by ID.</summary>
    /// <exception cref="DataStoreException">The store could not be read.</exception>
    public IReadOnlyList<Asset> GetAll() => assets.GetAll();

    /// <summary>Every office, ordered by name.</summary>
    /// <exception cref="DataStoreException">The store could not be read.</exception>
    public IReadOnlyList<Office> GetOffices() => offices.GetAll();

    /// <summary>
    /// Whether an asset already has <paramref name="serialNumber"/> (compared trimmed, as stored).
    /// Lets the UI re-prompt instead of losing a half-filled form.
    /// </summary>
    /// <exception cref="DataStoreException">The store could not be read.</exception>
    public bool SerialNumberExists(string serialNumber) => assets.SerialNumberExists(serialNumber.Trim());

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

    private T Add<T>(AssetDetails details, Func<Office, T> create)
        where T : Asset
    {
        ArgumentNullException.ThrowIfNull(details);

        var office = offices.FindById(details.OfficeId) ?? throw new OfficeNotFoundException(details.OfficeId);

        // Construct first: the constructor validates and trims, so the uniqueness check sees the
        // serial number exactly as it will be stored.
        var asset = create(office);
        if (assets.SerialNumberExists(asset.SerialNumber))
        {
            throw new DuplicateSerialNumberException(asset.SerialNumber);
        }

        assets.Add(asset);
        return asset;
    }
}
