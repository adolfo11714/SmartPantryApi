using SmartyPantryApi.Application.Domain;

namespace SmartPantryApi.Application.Infrastructure.Interfaces;

public interface IFoodPantryRepository
{
    public Task<DbFoodItem?> GetByIdAsync(int id, CancellationToken ct);
    public Task<DbFoodItem?> GetByNameAsync(string name, CancellationToken ct);
    public Task<DbCategory> GetCategoryByIdAsync(int id, CancellationToken ct);
}