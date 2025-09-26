namespace SmartyPantryApi.Application.Domain;

public sealed class FoodKeeperItem
{
    public string Name { get; set; } = string.Empty;
    public int? PantryDays { get; set; }
    public int? RefrigeratorDays { get; set; }
    public int? FreezerDays { get; set; }
}