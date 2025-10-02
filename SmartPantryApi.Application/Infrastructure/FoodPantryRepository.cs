using Dapper;
using SmartPantryApi.Application.Infrastructure.Interfaces;
using SmartyPantryApi.Application.Domain;

namespace SmartPantryApi.Application.Infrastructure;

public class FoodPantryRepository(IConfiguration configuration) : IFoodPantryRepository
{
    private string GetConnectionString()
    {
        return configuration.GetConnectionString("FoodPantry") ?? string.Empty;
    }

    public async Task<DbFoodItem?> GetByIdAsync(int id, CancellationToken ct)
    {
        await using var conn = new MySqlConnector.MySqlConnection(GetConnectionString());
        await conn.OpenAsync(ct);
        const string sql = "SELECT * FROM items WHERE ID = @id LIMIT 1";
        return await conn.QueryFirstOrDefaultAsync<DbFoodItem>(sql, new { id }, commandTimeout: 30);
    }

    public async Task<DbFoodItem?> GetByNameAsync(string name, CancellationToken ct)
    {
        await using var conn = new MySqlConnector.MySqlConnection(GetConnectionString());
        await conn.OpenAsync(ct);
        const string sql = "SELECT * FROM items WHERE Name = @name LIMIT 1";
        return await conn.QueryFirstOrDefaultAsync<DbFoodItem>(sql, new { name }, commandTimeout: 30);
    }

    public async Task<DbCategory> GetCategoryByIdAsync(int id, CancellationToken ct)
    {
        await using var conn = new MySqlConnector.MySqlConnection(GetConnectionString());
        await conn.OpenAsync(ct);
        const string sql = "SELECT * FROM categories WHERE ID = @id LIMIT 1";
        var result = await conn.QueryFirstOrDefaultAsync<DbCategory>(sql, new { id }, commandTimeout: 30);
        if (result == null) return new DbCategory();
        return result;
    }
}

