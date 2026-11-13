using AssetTracker.Core.Abstractions;
using AssetTracker.Core.Enums;

namespace AssetTracker.Infrastructure.Currency;

/// <summary>
/// Approximate, fixed euro rates. Only the offline fallback: <see cref="ApiCurrencyProvider"/>
/// is the source of current rates when the network is available.
/// </summary>
public sealed class HardcodedCurrencyProvider : ICurrencyProvider
{
    private static readonly Dictionary<CurrencyCode, decimal> s_rates = new()
    {
        [CurrencyCode.EUR] = 1.0m,
        [CurrencyCode.SEK] = 11.27m,
        [CurrencyCode.USD] = 1.15m,
        [CurrencyCode.TRY] = 55.88m,
    };

    public decimal GetRate(CurrencyCode currency) => s_rates[currency];
}
