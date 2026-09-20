namespace AssetTracker.Services;

internal enum CurrencyRateSource
{
    TodayCache,
    LiveApi,
    StaleCache,
    Hardcoded,
}
