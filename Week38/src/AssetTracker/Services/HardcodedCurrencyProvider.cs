using AssetTracker.Models;

namespace AssetTracker.Services;

// Approximate, fixed EUR-based rates - offline fallback only. ApiCurrencyProvider
// is the source of truth for accurate/current rates when a network is available.
internal sealed class HardcodedCurrencyProvider : ICurrencyProvider
{
    private static readonly Dictionary<CurrencyCode, decimal> Rates = new()
    {
        [CurrencyCode.EUR] = 1.0m,
        [CurrencyCode.SEK] = 11.27m,
        [CurrencyCode.USD] = 1.15m,
        [CurrencyCode.TRY] = 55.88m,
    };

    public decimal GetRate(CurrencyCode currency)
    {
        return Rates[currency];
    }
}
