using AssetTracker.Core.Enums;

namespace AssetTracker.Core.Abstractions;

/// <summary>Exchange rates against the euro.</summary>
public interface ICurrencyProvider
{
    /// <summary>
    /// How many units of <paramref name="currency"/> one euro buys, e.g. 11.2 for SEK. Always
    /// greater than 0, and exactly 1 for <see cref="CurrencyCode.EUR"/>. Never throws.
    /// </summary>
    decimal GetRate(CurrencyCode currency);
}
