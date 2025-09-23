using System.Text.Json;

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

    public DateOnly? CalculateExpiration(DateOnly purchasedOn, string productName, StorageLocation storageLocation)
    {
        if (!_nameToItem.TryGetValue(productName.Trim(), out var item))
        {
            return null;
        }

        var days = storageLocation switch
        {
            StorageLocation.Pantry => item.PantryDays,
            StorageLocation.Refrigerator => item.RefrigeratorDays,
            StorageLocation.Freezer => item.FreezerDays,
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

        if (!root.TryGetProperty("sheets", out var sheets))
        {
            return result;
        }

        foreach (var sheet in sheets.EnumerateArray())
        {
            if (!sheet.TryGetProperty("Name", out var nameProp)) continue;
            var sheetName = nameProp.GetString();
            if (string.IsNullOrWhiteSpace(sheetName)) continue;

            // Heuristic: find the sheet that contains product info with storage durations
            // We look for a header row that includes keys like "Product", "Pantry", "Refrigerator", "Freezer"
            if (!sheet.TryGetProperty("data", out var dataProp)) continue;

            foreach (var row in dataProp.EnumerateArray())
            {
                // Each row is an array of single-key objects
                var rowDict = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
                foreach (var cell in row.EnumerateArray())
                {
                    foreach (var prop in cell.EnumerateObject())
                    {
                        rowDict[prop.Name] = prop.Value.ValueKind == JsonValueKind.String ? prop.Value.GetString() : prop.Value.ToString();
                    }
                }

                // We expect something like Name, Pantry_Days, Refrigerator_Days, Freezer_Days
                var productName = GetFirstNonEmpty(rowDict, new[] { "Product", "Product_Name", "Name", "name", "product_name", "name_title" });
                if (string.IsNullOrWhiteSpace(productName)) continue;

                var pantryDays = ParseFirstInt(rowDict, new[] { "Pantry", "Pantry_Days", "Pantry_Duration_Days" });
                var fridgeDays = ParseFirstInt(rowDict, new[] { "Refrigerator", "Refrigerator_Days", "Refrigerate_Days" });
                var freezerDays = ParseFirstInt(rowDict, new[] { "Freezer", "Freezer_Days" });

                if (pantryDays is null && fridgeDays is null && freezerDays is null)
                {
                    continue;
                }

                result[productName] = new FoodKeeperItem
                {
                    Name = productName,
                    PantryDays = pantryDays,
                    RefrigeratorDays = fridgeDays,
                    FreezerDays = freezerDays
                };
            }
        }

        return result;
    }

    private static string? GetFirstNonEmpty(Dictionary<string, string?> row, IEnumerable<string> keys)
    {
        foreach (var key in keys)
        {
            if (row.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value))
            {
                return value?.Trim();
            }
        }
        return null;
    }

    private static int? ParseFirstInt(Dictionary<string, string?> row, IEnumerable<string> keys)
    {
        foreach (var key in keys)
        {
            if (row.TryGetValue(key, out var value) && int.TryParse(value, out var i))
            {
                return i;
            }
        }
        return null;
    }
}

public enum StorageLocation
{
    Pantry,
    Refrigerator,
    Freezer
}

public sealed class FoodKeeperItem
{
    public string Name { get; set; } = string.Empty;
    public int? PantryDays { get; set; }
    public int? RefrigeratorDays { get; set; }
    public int? FreezerDays { get; set; }
}


