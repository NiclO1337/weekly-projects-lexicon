using System.Globalization;
using System.Text.Json;
using AssetTracker.Models;

namespace AssetTracker.Services;

/// <summary>
/// Fetches EUR-based rates from the free, no-key Frankfurter API (ECB reference rates),
/// caching the result in a single file keyed by date so repeated runs on the same day
/// never re-call the API. On any failure (network, parsing, cache read/write) it falls
/// back to HardcodedCurrencyProvider rather than throwing - callers check UsedFallback
/// to decide whether to inform the user, since this layer can't touch the console.
/// </summary>
internal sealed class ApiCurrencyProvider : ICurrencyProvider
{
    private const string ApiUrl = "https://api.frankfurter.dev/v1/latest?base=EUR&symbols=SEK,USD,TRY";

    private static readonly HttpClient HttpClient = new() { Timeout = TimeSpan.FromSeconds(5) };
    private static readonly JsonSerializerOptions SerializerOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    private readonly HardcodedCurrencyProvider fallback = new();
    private readonly Dictionary<string, decimal> rates;

    internal bool UsedFallback { get; }

    internal ApiCurrencyProvider(string cacheFilePath)
    {
        string today = DateTime.Today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

        ExchangeRateData? cached = TryReadCache(cacheFilePath);
        if (cached is not null && cached.Date == today)
        {
            rates = cached.Rates;
            return;
        }

        ExchangeRateData? fetched = TryFetchLive();
        if (fetched is not null)
        {
            WriteCache(cacheFilePath, fetched);
            rates = fetched.Rates;
            return;
        }

        UsedFallback = true;
        rates = [];
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

    private static ExchangeRateData? TryReadCache(string cacheFilePath)
    {
        try
        {
            if (!File.Exists(cacheFilePath))
            {
                return null;
            }

            string json = File.ReadAllText(cacheFilePath);
            return JsonSerializer.Deserialize<ExchangeRateData>(json, SerializerOptions);
        }
        catch (Exception ex) when (ex is JsonException or IOException)
        {
            return null;
        }
    }

    private static ExchangeRateData? TryFetchLive()
    {
        try
        {
            string json = HttpClient.GetStringAsync(ApiUrl).GetAwaiter().GetResult();
            return JsonSerializer.Deserialize<ExchangeRateData>(json, SerializerOptions);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
        {
            return null;
        }
    }

    private static void WriteCache(string cacheFilePath, ExchangeRateData data)
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

    private sealed record ExchangeRateData(string Date, Dictionary<string, decimal> Rates);
}
