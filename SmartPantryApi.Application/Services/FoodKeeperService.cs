using System.Text.Json;
using Dapper;
using SmartyPantryApi.Application.Domain;
using SmartyPantryApi.Application.Domain.Enums;

namespace SmartPantryApi.Application.Services;

public class FoodKeeperService
{
    private readonly Dictionary<string, FoodKeeperItem> _nameToItem;

    public FoodKeeperService(IHostEnvironment hostEnvironment)
    {
        var dataPath = Path.Combine(hostEnvironment.ContentRootPath, "Resources", "product.json");
        _nameToItem = LoadFoodKeeper(dataPath);
    }

    public FoodKeeperItem? GetItemByName(string productName)
    {
        if (string.IsNullOrWhiteSpace(productName))
        {
            return null;
        }

        if (_nameToItem.TryGetValue(productName.Trim(), out var item))
        {
            return item;
        }

        return null;
    }

    public DateOnly? CalculateExpiration(DateOnly purchasedOn, string productName, StorageLocationEnum storageLocationEnum)
    {
        if (!_nameToItem.TryGetValue(productName.Trim(), out var item))
        {
            return null;
        }

        var days = storageLocationEnum switch
        {
            StorageLocationEnum.Pantry => item.PantryDays,
            StorageLocationEnum.Refrigerator => item.RefrigeratorDays,
            StorageLocationEnum.Freezer => item.FreezerDays,
            _ => null
        };

        if (days is null)
        {
            return null;
        }

        return purchasedOn.AddDays(days.Value);
    }

    private static Dictionary<string, FoodKeeperItem> LoadFoodKeeper(string path)
    {
        using var stream = File.OpenRead(path);
        using var doc = JsonDocument.Parse(stream);

        var root = doc.RootElement;
        var result = new Dictionary<string, FoodKeeperItem>(StringComparer.OrdinalIgnoreCase);

        // product.json schema: { "name": "Product", "data": [ [ {"Field": value}, ... ], ... ] }
        if (!root.TryGetProperty("data", out var dataRows) || dataRows.ValueKind != JsonValueKind.Array)
        {
            return result;
        }

        foreach (var row in dataRows.EnumerateArray())
        {
            if (row.ValueKind != JsonValueKind.Array)
            {
                continue;
            }

            // Build row dictionary from cells (each cell is an object with a single property)
            var rowDict = new Dictionary<string, JsonElement>(StringComparer.OrdinalIgnoreCase);
            foreach (var cell in row.EnumerateArray())
            {
                if (cell.ValueKind != JsonValueKind.Object) continue;
                foreach (var prop in cell.EnumerateObject())
                {
                    rowDict[prop.Name] = prop.Value;
                }
            }

            var productName = GetString(rowDict, "Name");
            if (string.IsNullOrWhiteSpace(productName))
            {
                continue;
            }

            // Prefer DOP_* fields when available; otherwise fall back to regular fields
            var pantryDays = ComputeDurationDays(rowDict, "DOP_Pantry_") ?? ComputeDurationDays(rowDict, "Pantry_");
            var fridgeDays = ComputeDurationDays(rowDict, "DOP_Refrigerate_") ?? ComputeDurationDays(rowDict, "Refrigerate_");
            var freezerDays = ComputeDurationDays(rowDict, "DOP_Freeze_") ?? ComputeDurationDays(rowDict, "Freeze_");

            result[productName] = new FoodKeeperItem
            {
                Name = productName,
                PantryDays = pantryDays,
                RefrigeratorDays = fridgeDays,
                FreezerDays = freezerDays
            };
        }

        return result;
    }

    private static string? GetString(Dictionary<string, JsonElement> row, string key)
    {
        if (!row.TryGetValue(key, out var el)) return null;
        return el.ValueKind switch
        {
            JsonValueKind.String => el.GetString(),
            JsonValueKind.Number => el.TryGetInt32(out var i) ? i.ToString() : el.ToString(),
            JsonValueKind.Null => null,
            _ => el.ToString()
        };
    }

    private static double? GetNumber(Dictionary<string, JsonElement> row, string key)
    {
        if (!row.TryGetValue(key, out var el)) return null;
        if (el.ValueKind == JsonValueKind.Number)
        {
            if (el.TryGetDouble(out var d)) return d;
        }
        if (el.ValueKind == JsonValueKind.String && double.TryParse(el.GetString(), out var parsed))
        {
            return parsed;
        }
        return null;
    }

    private static int? ComputeDurationDays(Dictionary<string, JsonElement> row, string prefix)
    {
        // Expect keys like <prefix>Min, <prefix>Max, <prefix>Metric
        var min = GetNumber(row, prefix + "Min");
        var max = GetNumber(row, prefix + "Max");
        var metric = GetString(row, prefix + "Metric");

        if (min is null && max is null)
        {
            return null;
        }

        // Prefer max if present; else min
        var value = max ?? min ?? 0.0;
        var days = value * MetricToDays(metric);
        var rounded = (int)Math.Round(days, MidpointRounding.AwayFromZero);
        return rounded > 0 ? rounded : (int?)null;
    }

    private static double MetricToDays(string? metric)
    {
        if (string.IsNullOrWhiteSpace(metric)) return 1.0; // assume days if unspecified
        return metric.Trim().ToLowerInvariant() switch
        {
            "day" or "days" => 1.0,
            "week" or "weeks" => 7.0,
            "month" or "months" => 30.0,
            "year" or "years" => 365.0,
            _ => 1.0
        };
    }
}


