using AssetTracker.Core.Abstractions;
using AssetTracker.Core.Enums;

namespace AssetTracker.Core.Services;

/// <summary>The one place an amount is converted to euros.</summary>
public static class CurrencyConversion
{
    /// <summary>
    /// Converts <paramref name="amount"/> in <paramref name="currency"/> to euros: the amount divided
    /// by how many units of that currency one euro buys, rounded to cents.
    /// </summary>
    public static decimal ToEur(this ICurrencyProvider rates, decimal amount, CurrencyCode currency) =>
        Math.Round(amount / rates.GetRate(currency), 2, MidpointRounding.AwayFromZero);
}
