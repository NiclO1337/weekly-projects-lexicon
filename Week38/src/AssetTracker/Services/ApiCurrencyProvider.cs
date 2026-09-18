using System.Globalization;
using System.Text.Json;
using AssetTracker.Models;

namespace AssetTracker.Services;

/// <summary>
/// Fetches EUR-based rates from the free, no-key Frankfurter API (ECB reference rates),
/// caching the result in a single file keyed by date so repeated runs on the same day
/// never re-call the API. On failure it falls back first to whatever was last cached
/// (even if stale), and only to HardcodedCurrencyProvider if no cache exists at all -
/// this keeps offline behavior from drifting years out of date as long as the API has
/// been reachable at least once. Never throws; callers check Source to decide what to
/// tell the user, since this layer can't touch the console.
/// </summary>
internal sealed class ApiCurrencyProvider : ICurrencyProvider
{
    private const string ApiUrl = "https://api.frankfurter.dev/v1/latest?base=EUR&symbols=SEK,USD,TRY";

    private static readonly HttpClient HttpClient = new() { Timeout = TimeSpan.FromSeconds(5) };
    private static readonly JsonSerializerOptions SerializerOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    private readonly HardcodedCurrencyProvider fallback = new();
    private readonly Dictionary<string, decimal> rates;

    internal CurrencyRateSource Source { get; }

    internal string? CachedDate { get; }

    internal ApiCurrencyProvider(string cacheFilePath)
    {
        string today = DateTime.Today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

        ExchangeRateCache? cached = TryReadCache(cacheFilePath);
        if (cached is not null && cached.FetchedOn == today)
        {
            rates = cached.Rates;
            Source = CurrencyRateSource.TodayCache;
            CachedDate = cached.FetchedOn;
            return;
        }

        ApiResponse? fetched = TryFetchLive();
        if (fetched is not null)
        {
            WriteCache(cacheFilePath, new ExchangeRateCache(today, fetched.Rates));
            rates = fetched.Rates;
            Source = CurrencyRateSource.LiveApi;
            return;
        }

        if (cached is not null)
        {
            rates = cached.Rates;
            Source = CurrencyRateSource.StaleCache;
            CachedDate = cached.FetchedOn;
            return;
        }

        rates = [];
        Source = CurrencyRateSource.Hardcoded;
    }

    public decimal GetRate(CurrencyCode currency)
    {
        if (currency == CurrencyCode.EUR)
        {
            return 1.0m;
        }

        if (rates.TryGetValue(currency.ToString(), out decimal rate))
        {
            return rate;
        }

        return fallback.GetRate(currency);
    }

    private static ExchangeRateCache? TryReadCache(string cacheFilePath)
    {
        try
        {
            if (!File.Exists(cacheFilePath))
            {
                return null;
            }

            string json = File.ReadAllText(cacheFilePath);
            return JsonSerializer.Deserialize<ExchangeRateCache>(json, SerializerOptions);
        }
        catch (Exception ex) when (ex is JsonException or IOException)
        {
            return null;
        }
    }

    private static ApiResponse? TryFetchLive()
    {
        try
        {
            string json = HttpClient.GetStringAsync(ApiUrl).GetAwaiter().GetResult();
            return JsonSerializer.Deserialize<ApiResponse>(json, SerializerOptions);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
        {
            return null;
        }
    }

    private static void WriteCache(string cacheFilePath, ExchangeRateCache data)
    {
        try
        {
            string? directory = Path.GetDirectoryName(cacheFilePath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string json = JsonSerializer.Serialize(data, SerializerOptions);
            File.WriteAllText(cacheFilePath, json);
        }
        catch (IOException)
        {
            // Cache write failure isn't fatal - the fetched rates are still used for this run.
        }
    }

    // The API's own "date" is the ECB rate-effective date (only updates on ECB business
    // days), not "when we fetched it" - comparing that against DateTime.Today would mean
    // never matching over a weekend and re-calling the API on every run. FetchedOn is our
    // own calendar-day stamp, recorded separately, used purely for cache-freshness.
    private sealed record ApiResponse(string Date, Dictionary<string, decimal> Rates);

    private sealed record ExchangeRateCache(string FetchedOn, Dictionary<string, decimal> Rates);
}
