namespace AssetTracker.Core.Models;

/// <summary>
/// The editable fields of an asset, as entered by the user. Used both to create an asset and to
/// update one. Carries no currency: an asset's currency comes from its office when it is created.
/// </summary>
/// <param name="Brand">Manufacturer, e.g. "Dell".</param>
/// <param name="Model">Model name, e.g. "XPS 13".</param>
/// <param name="SerialNumber">Unique serial number.</param>
/// <param name="PurchaseDate">Purchase date; not in the future.</param>
/// <param name="WarrantyExpirationDate">Warranty end; not before <paramref name="PurchaseDate"/>.</param>
/// <param name="PurchasePrice">What was paid, in the asset's local currency.</param>
/// <param name="OfficeId">The office the asset belongs to.</param>
/// <param name="AssignedTo">Optional employee or username; blank means unassigned.</param>
public sealed record AssetDetails(
    string Brand,
    string Model,
    string SerialNumber,
    DateOnly PurchaseDate,
    DateOnly WarrantyExpirationDate,
    decimal PurchasePrice,
    int OfficeId,
    string? AssignedTo);
