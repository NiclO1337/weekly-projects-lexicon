using System.Globalization;
using AssetTracker.Exceptions;
using AssetTracker.Models;

namespace AssetTracker.Services;

internal sealed class AssetService(IAssetRepository repository)
{
    internal const int PageSize = 5;

    internal void AddAsset(Asset asset)
    {
        asset.Id = repository.GetNextId();
        repository.Add(asset);
    }

    internal Asset GetById(int id)
    {
        return repository.GetAll().FirstOrDefault(a => a.Id == id) ?? throw new AssetNotFoundException(id);
    }

    internal void ReplaceAsset(Asset asset)
    {
        repository.Replace(asset);
    }

    internal void RemoveAsset(int id)
    {
        repository.Remove(id);
    }

    internal IReadOnlyList<Asset> GetSortedAssets(AssetSortMode sortMode)
    {
        IEnumerable<Asset> assets = repository.GetAll();

        return sortMode switch
        {
            AssetSortMode.AssetType => assets
                .OrderBy(a => a.GetCategoryLabel(), StringComparer.OrdinalIgnoreCase)
                .ThenBy(a => a.PurchaseDate)
                .ToList(),
            AssetSortMode.EndOfLife => assets
                .OrderBy(a => a.GetEndOfLifeDate())
                .ToList(),
            _ => assets
                .OrderBy(a => a.Office.Name, StringComparer.OrdinalIgnoreCase)
                .ThenBy(a => a.PurchaseDate)
                .ToList(),
        };
    }

    internal IReadOnlyList<Asset> SearchByBrand(string searchTerm)
    {
        return repository.GetAll()
            .Where(a => a.Brand.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    internal IReadOnlyList<Asset> SearchByModel(string searchTerm)
    {
        return repository.GetAll()
            .Where(a => a.Model.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    internal AssetsPage GetAssetsPage(AssetSortMode sortMode, int pageNumber)
    {
        IReadOnlyList<Asset> sorted = GetSortedAssets(sortMode);
        int totalPages = Math.Max(1, (int)Math.Ceiling(sorted.Count / (double)PageSize));
        int clampedPage = Math.Clamp(pageNumber, 1, totalPages);

        IReadOnlyList<Asset> pageItems = sorted
            .Skip((clampedPage - 1) * PageSize)
            .Take(PageSize)
            .ToList();

        return new AssetsPage(pageItems, clampedPage, totalPages);
    }

    internal static int ExportToCsv(IReadOnlyList<Asset> assets, string filePath)
    {
        using StreamWriter writer = new(filePath);
        writer.WriteLine("Id,Type,Brand,Model,PurchaseDate,PriceEur,Office,EndOfLifeStatus");

        foreach (Asset asset in assets)
        {
            writer.WriteLine(string.Join(',',
                asset.Id.ToString(CultureInfo.InvariantCulture),
                EscapeCsvField(asset.GetCategoryLabel()),
                EscapeCsvField(asset.Brand),
                EscapeCsvField(asset.Model),
                asset.PurchaseDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                asset.PriceEur.ToString(CultureInfo.InvariantCulture),
                EscapeCsvField(asset.Office.Name),
                asset.GetEndOfLifeStatus().ToString()));
        }

        return assets.Count;
    }

    private static string EscapeCsvField(string value)
    {
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }

        return value;
    }
}
