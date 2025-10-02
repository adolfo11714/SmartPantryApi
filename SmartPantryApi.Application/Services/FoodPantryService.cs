using SmartPantryApi.Application.Infrastructure;
using SmartPantryApi.Application.Infrastructure.Interfaces;
using SmartPantryApi.Application.Services.Interfaces;
using SmartyPantryApi.Application.Domain;
using SmartyPantryApi.Application.Domain.DTO;
using SmartyPantryApi.Application.Domain.Enums;

namespace SmartPantryApi.Application.Services;

public class FoodPantryService(IFoodPantryRepository foodPantryRepository) : IFoodPantryService
{
    public async Task<FoodItemDTO?> GetByIdAsync(int id, CancellationToken ct)
    {
        var result = await foodPantryRepository.GetByIdAsync(id, ct);
        
        if (result == null) return null;

        int categoryId = result.Category_ID ?? 0;
        var category = await foodPantryRepository.GetCategoryByIdAsync(categoryId, ct);

        var foodItemDto = FoodItemMapper(result, category);

        return foodItemDto;
    }

    public async Task<FoodItemDTO?> GetByNameAsync(string name, CancellationToken ct)
    {
        var result = await foodPantryRepository.GetByNameAsync(name, ct);
        
        if (result == null) return null;
        
        int categoryId = result.Category_ID ?? 0;
        var category = await foodPantryRepository.GetCategoryByIdAsync(categoryId, ct);
        
        var foodItemDto = FoodItemMapper(result, category);
        
        return foodItemDto;
    }
    
    private static FoodItemDTO FoodItemMapper(DbFoodItem result, DbCategory category)
    {
        var storageLocationList = new List<StorageLocationGroup>();
        
        if (result.Pantry_Metric != null)
        {
            var storageLocation = new StorageLocationGroup
            {
                Name = "Pantry",
                Min = result.Pantry_Min,
                Max = result.Pantry_Max,
                Metric = result.Pantry_Metric,
                Tips = result.Pantry_tips
            };
            
            storageLocationList.Add(storageLocation);
        }
        if (result.DOP_Pantry_Metric != null)
        {
            var storageLocation = new StorageLocationGroup
            {
                Name = "DOP Pantry",
                Min = result.DOP_Pantry_Min,
                Max = result.DOP_Pantry_Max,
                Metric = result.DOP_Pantry_Metric,
                Tips = result.DOP_Pantry_tips
            };
            
            storageLocationList.Add(storageLocation);
        }
        if (result.Pantry_After_Opening_Metric != null)
        {
            var storageLocation = new StorageLocationGroup
            {
                Name = "Pantry After Opening",
                Min = result.Pantry_After_Opening_Min,
                Max = result.Pantry_After_Opening_Max,
                Metric = result.Pantry_After_Opening_Metric
            };
            
            storageLocationList.Add(storageLocation);
        }
        if (result.Refrigerate_Metric != null)
        {
            var storageLocation = new StorageLocationGroup
            {
                Name = "Refrigerate",
                Min = result.Refrigerate_Min,
                Max = result.Refrigerate_Max,
                Metric = result.Refrigerate_Metric,
                Tips =  result.Refrigerate_tips
            };
            
            storageLocationList.Add(storageLocation);
        }
        if (result.DOP_Refrigerate_Metric != null)
        {
            var storageLocation = new StorageLocationGroup
            {
                Name = "DOP Refrigerate",
                Min = result.DOP_Refrigerate_Min,
                Max = result.DOP_Refrigerate_Max,
                Metric = result.DOP_Refrigerate_Metric,
                Tips =  result.DOP_Refrigerate_tips
            };
            
            storageLocationList.Add(storageLocation);
        }
        if (result.Refrigerate_After_Opening_Metric != null)
        {
            var storageLocation = new StorageLocationGroup
            {
                Name = "Refrigerate After Opening",
                Min = result.Refrigerate_After_Opening_Min,
                Max = result.Refrigerate_After_Opening_Max,
                Metric = result.Refrigerate_After_Opening_Metric
            };
            
            storageLocationList.Add(storageLocation);
        }
        if (result.Refrigerate_After_Thawing_Metric != null)
        {
            var storageLocation = new StorageLocationGroup
            {
                Name = "Refrigerate After Thawing",
                Min = result.Refrigerate_After_Thawing_Min,
                Max = result.Refrigerate_After_Thawing_Max,
                Metric = result.Refrigerate_After_Thawing_Metric
            };
            
            storageLocationList.Add(storageLocation);
        }
        if (result.Freeze_Metric != null)
        {
            var storageLocation = new StorageLocationGroup
            {
                Name = "Freeze",
                Min = result.Freeze_Min,
                Max = result.Freeze_Max,
                Metric = result.Freeze_Metric,
                Tips = result.Freeze_Tips
            };
            
            storageLocationList.Add(storageLocation);
        }
        if (result.DOP_Freeze_Metric != null)
        {
            var storageLocation = new StorageLocationGroup
            {
                Name = "DOP Freeze",
                Min = result.DOP_Freeze_Min,
                Max = result.DOP_Freeze_Max,
                Metric = result.DOP_Freeze_Metric,
                Tips = result.DOP_Freeze_Tips
            };
            
            storageLocationList.Add(storageLocation);
        }
        
        var foodItemDto = new FoodItemDTO
        {
            Id = result.ID,
            Name = result.Name,
            Description = result.Name_subtitle,
            Category = category.Category_Name,
            Subcategory = category.Subcategory_Name,
            StorageLocation = storageLocationList
        };

        return foodItemDto;
    }
    public async Task<DbCategory> GetCategoryByIdAsync(int id, CancellationToken ct)
    {
        var result = await foodPantryRepository.GetCategoryByIdAsync(id, ct);

        return result;
    } 
}