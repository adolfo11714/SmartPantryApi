using SmartyPantryApi.Application.Domain;
using SmartyPantryApi.Application.Domain.DTO;

namespace SmartPantryApi.Application.Services.Interfaces;

public interface IFoodPantryService
{
    public Task<FoodItemDTO?> GetByIdAsync(int id, CancellationToken ct);
    public Task<FoodItemDTO?> GetByNameAsync(string name, CancellationToken ct);
}