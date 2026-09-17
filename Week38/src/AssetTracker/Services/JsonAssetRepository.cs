using System.Text.Json;
using System.Text.Json.Serialization;
using AssetTracker.Exceptions;
using AssetTracker.Models;

namespace AssetTracker.Services;

internal sealed class JsonAssetRepository : IAssetRepository
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() },
    };

    private readonly string filePath;
    private readonly List<Asset> assets;

    internal JsonAssetRepository(string filePath, bool skipLoad = false)
    {
        this.filePath = filePath;
        assets = skipLoad ? [] : Load();
    }

    public IReadOnlyList<Asset> GetAll()
    {
        return assets;
    }

    public int GetNextId()
    {
        return assets.Count > 0 ? assets.Max(a => a.Id) + 1 : 1;
    }

    public void Add(Asset asset)
    {
        if (assets.Any(a => a.Id == asset.Id))
        {
            throw new DuplicateAssetIdException(asset.Id);
        }

        assets.Add(asset);
        Save();
    }

    public void Replace(Asset asset)
    {
        int index = assets.FindIndex(a => a.Id == asset.Id);
        if (index < 0)
        {
            throw new AssetNotFoundException(asset.Id);
        }

        assets[index] = asset;
        Save();
    }

    public void Remove(int id)
    {
        int index = assets.FindIndex(a => a.Id == id);
        if (index < 0)
        {
            throw new AssetNotFoundException(id);
        }

        assets.RemoveAt(index);
        Save();
    }

    private List<Asset> Load()
    {
        if (!File.Exists(filePath))
        {
            return [];
        }

        string json = File.ReadAllText(filePath);
        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

        try
        {
            return JsonSerializer.Deserialize<List<Asset>>(json, SerializerOptions) ?? [];
        }
        catch (JsonException ex)
        {
            throw new InvalidAssetDataException($"Could not read {filePath}: {ex.Message}");
        }
    }

    private void Save()
    {
        string? directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        string json = JsonSerializer.Serialize(assets, SerializerOptions);
        File.WriteAllText(filePath, json);
    }
}
