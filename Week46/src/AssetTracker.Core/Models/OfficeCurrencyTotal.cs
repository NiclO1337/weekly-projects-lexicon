using AssetTracker.Core.Enums;

namespace AssetTracker.Core.Models;

/// <summary>
/// The assets of one office priced in one currency, counted and summed by the database. An office
/// has more than one row only if assets bought elsewhere have moved there.
/// </summary>
/// <param name="OfficeId">The office.</param>
/// <param name="Currency">The currency of <paramref name="TotalPrice"/>.</param>
/// <param name="AssetCount">How many assets.</param>
/// <param name="TotalPrice">Their purchase prices added up, in <paramref name="Currency"/>.</param>
public sealed record OfficeCurrencyTotal(int OfficeId, CurrencyCode Currency, int AssetCount, decimal TotalPrice);
