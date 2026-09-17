using AssetTracker.Models;

namespace AssetTracker.Services;

internal sealed record AssetsPage(IReadOnlyList<Asset> Items, int PageNumber, int TotalPages);
