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

public sealed class DbFoodItem
{
    public int ID { get; set; }
    public int? Category_ID { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Name_subtitle { get; set; }
    public string? Keywords { get; set; }
    public int? Pantry_Min { get; set; }
    public int? Pantry_Max { get; set; }
    public string? Pantry_Metric { get; set; }
    public string? Pantry_tips { get; set; }
    public int? DOP_Pantry_Min { get; set; }
    public int? DOP_Pantry_Max { get; set; }
    public string? DOP_Pantry_Metric { get; set; }
    public string? DOP_Pantry_tips { get; set; }
    public int? Pantry_After_Opening_Min { get; set; }
    public int? Pantry_After_Opening_Max { get; set; }
    public string? Pantry_After_Opening_Metric { get; set; }
    public int? Refrigerate_Min { get; set; }
    public int? Refrigerate_Max { get; set; }
    public string? Refrigerate_Metric { get; set; }
    public string? Refrigerate_tips { get; set; }
    public int? DOP_Refrigerate_Min { get; set; }
    public int? DOP_Refrigerate_Max { get; set; }
    public string? DOP_Refrigerate_Metric { get; set; }
    public string? DOP_Refrigerate_tips { get; set; }
    public int? Refrigerate_After_Opening_Min { get; set; }
    public int? Refrigerate_After_Opening_Max { get; set; }
    public string? Refrigerate_After_Opening_Metric { get; set; }
    public int? Refrigerate_After_Thawing_Min { get; set; }
    public int? Refrigerate_After_Thawing_Max { get; set; }
    public string? Refrigerate_After_Thawing_Metric { get; set; }
    public int? Freeze_Min { get; set; }
    public int? Freeze_Max { get; set; }
    public string? Freeze_Metric { get; set; }
    public string? Freeze_Tips { get; set; }
    public int? DOP_Freeze_Min { get; set; }
    public int? DOP_Freeze_Max { get; set; }
    public string? DOP_Freeze_Metric { get; set; }
    public string? DOP_Freeze_Tips { get; set; }
}

public class ItemsRepository
{
    private readonly IConfiguration _configuration;

    public ItemsRepository(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    private string GetConnectionString()
    {
        return _configuration.GetConnectionString("FoodPantry") ?? string.Empty;
    }

    public async Task<DbFoodItem?> GetByIdAsync(int id, CancellationToken ct)
    {
        await using var conn = new MySqlConnector.MySqlConnection(GetConnectionString());
        await conn.OpenAsync(ct);
        const string sql = "SELECT * FROM items WHERE ID = @id LIMIT 1";
        await using var cmd = new MySqlConnector.MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id", id);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct)) return null;
        return Map(reader);
    }

    public async Task<DbFoodItem?> GetByNameAsync(string name, CancellationToken ct)
    {
        await using var conn = new MySqlConnector.MySqlConnection(GetConnectionString());
        await conn.OpenAsync(ct);
        const string sql = "SELECT * FROM items WHERE Name = @name LIMIT 1";
        await using var cmd = new MySqlConnector.MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@name", name);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct)) return null;
        return Map(reader);
    }

    private static DbFoodItem Map(System.Data.Common.DbDataReader r)
    {
        DbFoodItem item = new DbFoodItem();
        item.ID = r.GetInt32(r.GetOrdinal("ID"));
        item.Category_ID = r.IsDBNull(r.GetOrdinal("Category_ID")) ? null : r.GetInt32(r.GetOrdinal("Category_ID"));
        item.Name = r.GetString(r.GetOrdinal("Name"));
        item.Name_subtitle = r.IsDBNull(r.GetOrdinal("Name_subtitle")) ? null : r.GetString(r.GetOrdinal("Name_subtitle"));
        item.Keywords = r.IsDBNull(r.GetOrdinal("Keywords")) ? null : r.GetString(r.GetOrdinal("Keywords"));
        item.Pantry_Min = r.IsDBNull(r.GetOrdinal("Pantry_Min")) ? null : r.GetInt32(r.GetOrdinal("Pantry_Min"));
        item.Pantry_Max = r.IsDBNull(r.GetOrdinal("Pantry_Max")) ? null : r.GetInt32(r.GetOrdinal("Pantry_Max"));
        item.Pantry_Metric = r.IsDBNull(r.GetOrdinal("Pantry_Metric")) ? null : r.GetString(r.GetOrdinal("Pantry_Metric"));
        item.Pantry_tips = r.IsDBNull(r.GetOrdinal("Pantry_tips")) ? null : r.GetString(r.GetOrdinal("Pantry_tips"));
        item.DOP_Pantry_Min = r.IsDBNull(r.GetOrdinal("DOP_Pantry_Min")) ? null : r.GetInt32(r.GetOrdinal("DOP_Pantry_Min"));
        item.DOP_Pantry_Max = r.IsDBNull(r.GetOrdinal("DOP_Pantry_Max")) ? null : r.GetInt32(r.GetOrdinal("DOP_Pantry_Max"));
        item.DOP_Pantry_Metric = r.IsDBNull(r.GetOrdinal("DOP_Pantry_Metric")) ? null : r.GetString(r.GetOrdinal("DOP_Pantry_Metric"));
        item.DOP_Pantry_tips = r.IsDBNull(r.GetOrdinal("DOP_Pantry_tips")) ? null : r.GetString(r.GetOrdinal("DOP_Pantry_tips"));
        item.Pantry_After_Opening_Min = r.IsDBNull(r.GetOrdinal("Pantry_After_Opening_Min")) ? null : r.GetInt32(r.GetOrdinal("Pantry_After_Opening_Min"));
        item.Pantry_After_Opening_Max = r.IsDBNull(r.GetOrdinal("Pantry_After_Opening_Max")) ? null : r.GetInt32(r.GetOrdinal("Pantry_After_Opening_Max"));
        item.Pantry_After_Opening_Metric = r.IsDBNull(r.GetOrdinal("Pantry_After_Opening_Metric")) ? null : r.GetString(r.GetOrdinal("Pantry_After_Opening_Metric"));
        item.Refrigerate_Min = r.IsDBNull(r.GetOrdinal("Refrigerate_Min")) ? null : r.GetInt32(r.GetOrdinal("Refrigerate_Min"));
        item.Refrigerate_Max = r.IsDBNull(r.GetOrdinal("Refrigerate_Max")) ? null : r.GetInt32(r.GetOrdinal("Refrigerate_Max"));
        item.Refrigerate_Metric = r.IsDBNull(r.GetOrdinal("Refrigerate_Metric")) ? null : r.GetString(r.GetOrdinal("Refrigerate_Metric"));
        item.Refrigerate_tips = r.IsDBNull(r.GetOrdinal("Refrigerate_tips")) ? null : r.GetString(r.GetOrdinal("Refrigerate_tips"));
        item.DOP_Refrigerate_Min = r.IsDBNull(r.GetOrdinal("DOP_Refrigerate_Min")) ? null : r.GetInt32(r.GetOrdinal("DOP_Refrigerate_Min"));
        item.DOP_Refrigerate_Max = r.IsDBNull(r.GetOrdinal("DOP_Refrigerate_Max")) ? null : r.GetInt32(r.GetOrdinal("DOP_Refrigerate_Max"));
        item.DOP_Refrigerate_Metric = r.IsDBNull(r.GetOrdinal("DOP_Refrigerate_Metric")) ? null : r.GetString(r.GetOrdinal("DOP_Refrigerate_Metric"));
        item.DOP_Refrigerate_tips = r.IsDBNull(r.GetOrdinal("DOP_Refrigerate_tips")) ? null : r.GetString(r.GetOrdinal("DOP_Refrigerate_tips"));
        item.Refrigerate_After_Opening_Min = r.IsDBNull(r.GetOrdinal("Refrigerate_After_Opening_Min")) ? null : r.GetInt32(r.GetOrdinal("Refrigerate_After_Opening_Min"));
        item.Refrigerate_After_Opening_Max = r.IsDBNull(r.GetOrdinal("Refrigerate_After_Opening_Max")) ? null : r.GetInt32(r.GetOrdinal("Refrigerate_After_Opening_Max"));
        item.Refrigerate_After_Opening_Metric = r.IsDBNull(r.GetOrdinal("Refrigerate_After_Opening_Metric")) ? null : r.GetString(r.GetOrdinal("Refrigerate_After_Opening_Metric"));
        item.Refrigerate_After_Thawing_Min = r.IsDBNull(r.GetOrdinal("Refrigerate_After_Thawing_Min")) ? null : r.GetInt32(r.GetOrdinal("Refrigerate_After_Thawing_Min"));
        item.Refrigerate_After_Thawing_Max = r.IsDBNull(r.GetOrdinal("Refrigerate_After_Thawing_Max")) ? null : r.GetInt32(r.GetOrdinal("Refrigerate_After_Thawing_Max"));
        item.Refrigerate_After_Thawing_Metric = r.IsDBNull(r.GetOrdinal("Refrigerate_After_Thawing_Metric")) ? null : r.GetString(r.GetOrdinal("Refrigerate_After_Thawing_Metric"));
        item.Freeze_Min = r.IsDBNull(r.GetOrdinal("Freeze_Min")) ? null : r.GetInt32(r.GetOrdinal("Freeze_Min"));
        item.Freeze_Max = r.IsDBNull(r.GetOrdinal("Freeze_Max")) ? null : r.GetInt32(r.GetOrdinal("Freeze_Max"));
        item.Freeze_Metric = r.IsDBNull(r.GetOrdinal("Freeze_Metric")) ? null : r.GetString(r.GetOrdinal("Freeze_Metric"));
        item.Freeze_Tips = r.IsDBNull(r.GetOrdinal("Freeze_Tips")) ? null : r.GetString(r.GetOrdinal("Freeze_Tips"));
        item.DOP_Freeze_Min = r.IsDBNull(r.GetOrdinal("DOP_Freeze_Min")) ? null : r.GetInt32(r.GetOrdinal("DOP_Freeze_Min"));
        item.DOP_Freeze_Max = r.IsDBNull(r.GetOrdinal("DOP_Freeze_Max")) ? null : r.GetInt32(r.GetOrdinal("DOP_Freeze_Max"));
        item.DOP_Freeze_Metric = r.IsDBNull(r.GetOrdinal("DOP_Freeze_Metric")) ? null : r.GetString(r.GetOrdinal("DOP_Freeze_Metric"));
        item.DOP_Freeze_Tips = r.IsDBNull(r.GetOrdinal("DOP_Freeze_Tips")) ? null : r.GetString(r.GetOrdinal("DOP_Freeze_Tips"));
        return item;
    }
}


