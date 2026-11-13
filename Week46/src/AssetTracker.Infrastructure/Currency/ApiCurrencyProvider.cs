using System.Globalization;
using System.Text.Json;
using AssetTracker.Core.Abstractions;
using AssetTracker.Core.Enums;

namespace AssetTracker.Infrastructure.Currency;

/// <summary>
/// Euro rates from the free, keyless Frankfurter API (ECB reference rates), cached in a file
/// stamped with the day they were fetched, so the API is called at most once a day. If the API
/// can't be reached it uses the last cached rates, however old, and only without any cache the
/// built-in <see cref="HardcodedCurrencyProvider"/> rates. Never throws: <see cref="Source"/>
/// tells the caller which rates are in use, since this layer doesn't write to the console.
/// </summary>
public sealed class ApiCurrencyProvider : ICurrencyProvider
{
    private const string ApiUrl = "https://api.frankfurter.dev/v1/latest?base=EUR&symbols=SEK,USD,TRY";
    private const string DateFormat = "yyyy-MM-dd";

    private static readonly HttpClient s_httpClient = new() { Timeout = TimeSpan.FromSeconds(5) };
    private static readonly JsonSerializerOptions s_serializerOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    private readonly HardcodedCurrencyProvider _fallback = new();
    private readonly Dictionary<string, decimal> _rates;

    /// <summary>Loads the rates once, trying today's cache, the API, an older cache, then the built-in rates.</summary>
    public ApiCurrencyProvider()
    {
        var today = DateTime.Today.ToString(DateFormat, CultureInfo.InvariantCulture);

        var cached = TryReadCache();
        if (cached is not null && cached.FetchedOn == today)
        {
            (_rates, Source, CachedDate) = (cached.Rates, CurrencyRateSource.TodayCache, cached.FetchedOn);
            return;
        }

        var fetched = TryFetchLive();
        if (fetched is not null)
        {
            WriteCache(new ExchangeRateCache(today, fetched));
            (_rates, Source) = (fetched, CurrencyRateSource.LiveApi);
            return;
        }

        if (cached is not null)
        {
            (_rates, Source, CachedDate) = (cached.Rates, CurrencyRateSource.StaleCache, cached.FetchedOn);
            return;
        }

        (_rates, Source) = ([], CurrencyRateSource.Hardcoded);
    }

    /// <summary>Where the rates in use came from.</summary>
    public CurrencyRateSource Source { get; }

    /// <summary>The day the cached rates were fetched (yyyy-MM-dd), when they came from the cache.</summary>
    public string? CachedDate { get; }

    /// <summary>The loaded rate, or the built-in one if the API or cache didn't provide a usable rate.</summary>
    public decimal GetRate(CurrencyCode currency)
    {
        if (currency == CurrencyCode.EUR)
        {
            return 1.0m;
        }

        // Rates come from outside the app; a missing or non-positive one is never used.
        return _rates.TryGetValue(currency.ToString(), out var rate) && rate > 0
            ? rate
            : _fallback.GetRate(currency);
    }

    private static ExchangeRateCache? TryReadCache()
    {
        try
        {
            if (!File.Exists(AppPaths.ExchangeRateCacheFile))
            {
                return null;
            }

            // The file may have been edited or truncated: the JSON can hold nulls even though the record says it can't.
            var cache = JsonSerializer.Deserialize<ExchangeRateCache>(File.ReadAllText(AppPaths.ExchangeRateCacheFile), s_serializerOptions);
            return cache is { FetchedOn: not null, Rates: not null } ? cache : null;
        }
        catch (JsonException)
        {
            return null;
        }
        catch (IOException)
        {
            return null;
        }
        catch (UnauthorizedAccessException)
        {
            return null;
        }
    }

    private static Dictionary<string, decimal>? TryFetchLive()
    {
        try
        {
            var json = s_httpClient.GetStringAsync(ApiUrl).GetAwaiter().GetResult();
            return JsonSerializer.Deserialize<ApiResponse>(json, s_serializerOptions)?.Rates;
        }
        catch (HttpRequestException)
        {
            return null;
        }
        catch (TaskCanceledException)
        {
            // HttpClient reports its timeout this way.
            return null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static void WriteCache(ExchangeRateCache cache)
    {
        try
        {
            Directory.CreateDirectory(AppPaths.DataDirectory);
            File.WriteAllText(AppPaths.ExchangeRateCacheFile, JsonSerializer.Serialize(cache, s_serializerOptions));
        }
        catch (IOException)
        {
            // Not fatal: the fetched rates are still used for this run.
        }
        catch (UnauthorizedAccessException)
        {
            // Same as above.
        }
    }

    // The API's own "date" is the ECB's rate date, which only changes on ECB business days, so
    // comparing it with today would never match at weekends and call the API on every run.
    // FetchedOn is our own stamp of the day we fetched, used only to decide whether to refetch.
    private sealed record ApiResponse(string Date, Dictionary<string, decimal>? Rates);

    private sealed record ExchangeRateCache(string FetchedOn, Dictionary<string, decimal> Rates);
}
