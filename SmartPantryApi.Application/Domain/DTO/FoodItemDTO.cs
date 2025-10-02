namespace SmartyPantryApi.Application.Domain.DTO;

public class FoodItemDTO
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public string? Category { get; set; }
    public string? Subcategory { get; set; }
    
    public List<StorageLocationGroup> StorageLocation  { get; set; }
}

public class StorageLocationGroup
{
    public string Name { get; set; }
    public int? Min { get; set; }
    public int? Max { get; set; }
    public string? Metric { get; set; }
    public string? Tips { get; set; }
}