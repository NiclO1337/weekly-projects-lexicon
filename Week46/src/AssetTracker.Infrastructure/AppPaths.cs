namespace AssetTracker.Infrastructure;

/// <summary>Where Infrastructure keeps its runtime files.</summary>
internal static class AppPaths
{
    // AppContext.BaseDirectory is the console app's build output (src/AssetTracker.ConsoleApp/
    // bin/Debug/net10.0), which is the same however the app is started; the current directory
    // is not. Four levels up is src/, so the files land in src/AssetTracker.Infrastructure/Data.
    internal static string DataDirectory { get; } = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "AssetTracker.Infrastructure", "Data"));

    internal static string ExchangeRateCacheFile { get; } = Path.Combine(DataDirectory, "exchangeRates.json");
}
