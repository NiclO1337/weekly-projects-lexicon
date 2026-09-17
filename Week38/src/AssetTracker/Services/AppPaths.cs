namespace AssetTracker.Services;

internal static class AppPaths
{
    // AppContext.BaseDirectory is the build output folder (bin/Debug/net10.0), which
    // differs by launch method (dotnet run, IDE debug, running the .exe directly) if
    // computed from the current working directory instead. Walking up from there to the
    // project folder keeps Data/ in one predictable place: src/AssetTracker/Data.
    internal static string DataDirectory { get; } = Path.Combine(
        Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..")),
        "Data");
}
