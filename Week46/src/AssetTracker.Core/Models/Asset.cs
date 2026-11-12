using System.Diagnostics.CodeAnalysis;
using AssetTracker.Core.Enums;
using AssetTracker.Core.Services;

namespace AssetTracker.Core.Models;

/// <summary>
/// A company asset. <see cref="ComputerAsset"/> and <see cref="MobileAsset"/> are the concrete
/// kinds; <see cref="Category"/> and <see cref="TypeLabel"/> are the polymorphic hooks, so no
/// caller ever needs to ask which subclass it has. Every public constructor and
/// <see cref="Update"/> validate, so an asset in memory is always valid.
/// </summary>
public abstract class Asset
{
    /// <summary>The maximum allowed length of <see cref="Brand"/>.</summary>
    public const int MaxBrandLength = 50;

    /// <summary>The maximum allowed length of <see cref="Model"/>.</summary>
    public const int MaxModelLength = 50;

    /// <summary>The maximum allowed length of <see cref="SerialNumber"/>.</summary>
    public const int MaxSerialNumberLength = 50;

    /// <summary>The maximum allowed length of <see cref="AssignedTo"/>.</summary>
    public const int MaxAssignedToLength = 50;

    /// <summary>The maximum allowed value of <see cref="PurchasePrice"/>, in any currency.</summary>
    public const decimal MaxPurchasePrice = 150_000_000m;

    /// <summary>Database identity. Assigned by the database, never by code.</summary>
    public int Id { get; private set; }

    /// <summary>Trimmed, non-empty, at most <see cref="MaxBrandLength"/> characters.</summary>
    public string Brand { get; private set; }

    /// <summary>Trimmed, non-empty, at most <see cref="MaxModelLength"/> characters.</summary>
    public string Model { get; private set; }

    /// <summary>Trimmed, non-empty, at most <see cref="MaxSerialNumberLength"/> characters. Unique across assets.</summary>
    public string SerialNumber { get; private set; }

    /// <summary>Not in the future. The lifetime is counted from this date.</summary>
    public DateOnly PurchaseDate { get; private set; }

    /// <summary>Not before <see cref="PurchaseDate"/>.</summary>
    public DateOnly WarrantyExpirationDate { get; private set; }

    /// <summary>What was paid, in <see cref="Currency"/>. Greater than 0, at most <see cref="MaxPurchasePrice"/>, 2 decimals.</summary>
    public decimal PurchasePrice { get; private set; }

    /// <summary>
    /// The currency <see cref="PurchasePrice"/> was paid in. Taken from the office when the asset is
    /// created and never changed, even if the asset later moves to another office.
    /// </summary>
    public CurrencyCode Currency { get; private set; }

    /// <summary>The ID of <see cref="Office"/>.</summary>
    public int OfficeId { get; private set; }

    /// <summary>The office the asset belongs to.</summary>
    public Office Office { get; private set; }

    /// <summary>Optional employee or username, trimmed, at most <see cref="MaxAssignedToLength"/> characters.</summary>
    public string? AssignedTo { get; private set; }

    /// <summary>The broad kind of asset: computer or mobile.</summary>
    public abstract AssetCategory Category { get; }

    /// <summary>The specific type for display, e.g. "Laptop" or "Phone".</summary>
    public abstract string TypeLabel { get; }

    /// <summary>Creates a validated asset in <paramref name="office"/>, priced in the office's currency.</summary>
    /// <param name="details">The asset's fields. <see cref="AssetDetails.OfficeId"/> must match <paramref name="office"/>.</param>
    /// <param name="office">The office the asset belongs to.</param>
    /// <exception cref="ArgumentException">A text field is blank or too long, or the office doesn't match.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The price or a date is out of range.</exception>
    protected Asset(AssetDetails details, Office office)
    {
        ApplyDetails(details, office);
        Currency = office.Currency;
    }

#pragma warning disable CS8618 // EF Core sets every property when it loads the entity.
    /// <summary>For EF Core, which sets the properties itself when loading an asset.</summary>
    protected Asset()
    {
    }
#pragma warning restore CS8618

    /// <summary>
    /// Replaces the editable fields, re-validating exactly as the constructor does.
    /// <see cref="Id"/>, <see cref="Currency"/>, <see cref="Category"/> and the type never change.
    /// </summary>
    /// <param name="details">The new fields. <see cref="AssetDetails.OfficeId"/> must match <paramref name="office"/>.</param>
    /// <param name="office">The office the asset belongs to from now on.</param>
    /// <exception cref="ArgumentException">A text field is blank or too long, or the office doesn't match.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The price or a date is out of range.</exception>
    public void Update(AssetDetails details, Office office) => ApplyDetails(details, office);

    /// <summary>How close the asset is to the end of its lifetime, as of <paramref name="today"/>.</summary>
    public EndOfLifeStatus GetEndOfLifeStatus(DateOnly today) => AssetLifecycle.GetStatus(PurchaseDate, today);

    /// <summary>The status as text: "", "Monitor", "Upgrade soon" or "End of life".</summary>
    public string GetEndOfLifeStatusLabel(DateOnly today) => GetEndOfLifeStatus(today) switch
    {
        EndOfLifeStatus.Yellow => "Monitor",
        EndOfLifeStatus.Red => "Upgrade soon",
        EndOfLifeStatus.DarkRed => "End of life",
        _ => "",
    };

    [MemberNotNull(nameof(Brand), nameof(Model), nameof(SerialNumber), nameof(Office))]
    private void ApplyDetails(AssetDetails details, Office office)
    {
        ArgumentNullException.ThrowIfNull(details);
        ArgumentNullException.ThrowIfNull(office);

        if (details.OfficeId != office.Id)
        {
            throw new ArgumentException($"Office ID {details.OfficeId} does not match office {office.Id}.", nameof(office));
        }

        var today = DateOnly.FromDateTime(DateTime.Today);
        if (details.PurchaseDate > today)
        {
            throw new ArgumentOutOfRangeException(nameof(details), details.PurchaseDate, "Purchase date cannot be in the future.");
        }

        if (details.WarrantyExpirationDate < details.PurchaseDate)
        {
            throw new ArgumentOutOfRangeException(nameof(details), details.WarrantyExpirationDate, "Warranty expiration cannot be before the purchase date.");
        }

        // Validate everything before assigning anything, so a failed Update leaves the asset unchanged.
        var brand = RequireText(details.Brand, MaxBrandLength, "Brand");
        var model = RequireText(details.Model, MaxModelLength, "Model");
        var serialNumber = RequireText(details.SerialNumber, MaxSerialNumberLength, "Serial number");
        var assignedTo = string.IsNullOrWhiteSpace(details.AssignedTo)
            ? null
            : RequireText(details.AssignedTo, MaxAssignedToLength, "Assigned to");
        var purchasePrice = ValidatePrice(details.PurchasePrice);

        Brand = brand;
        Model = model;
        SerialNumber = serialNumber;
        AssignedTo = assignedTo;
        PurchasePrice = purchasePrice;
        PurchaseDate = details.PurchaseDate;
        WarrantyExpirationDate = details.WarrantyExpirationDate;
        OfficeId = office.Id;
        Office = office;
    }

    private static string RequireText(string value, int maxLength, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"{fieldName} cannot be blank.", nameof(value));
        }

        var trimmed = value.Trim();
        if (trimmed.Length > maxLength)
        {
            throw new ArgumentException($"{fieldName} cannot exceed {maxLength} characters.", nameof(value));
        }

        return trimmed;
    }

    private static decimal ValidatePrice(decimal price)
    {
        if (price <= 0 || price > MaxPurchasePrice)
        {
            throw new ArgumentOutOfRangeException(nameof(price), price, $"Price must be greater than 0 and at most {MaxPurchasePrice}.");
        }

        return Math.Round(price, 2, MidpointRounding.AwayFromZero);
    }
}
