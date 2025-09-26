namespace SmartyPantryApi.Application.Domain;

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