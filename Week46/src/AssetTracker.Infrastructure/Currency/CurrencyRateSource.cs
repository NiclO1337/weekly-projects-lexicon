namespace AssetTracker.Infrastructure.Currency;

/// <summary>Where <see cref="ApiCurrencyProvider"/> got the rates it is using.</summary>
public enum CurrencyRateSource
{
    /// <summary>Fetched from the API earlier today and read from the cache file.</summary>
    TodayCache,

    /// <summary>Fetched from the API just now.</summary>
    LiveApi,

    /// <summary>The API could not be reached; using the last cached rates, from an earlier day.</summary>
    StaleCache,

    /// <summary>The API could not be reached and there is no cache; using the built-in rates.</summary>
    Hardcoded,
}
