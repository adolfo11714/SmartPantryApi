using Dapper;
using SmartPantryApi.Application.Infrastructure.Interfaces;
using SmartPantryApi.Application.Services;
using SmartyPantryApi.Application.Domain;

namespace SmartPantryApi.Application.Infrastructure;

public class FoodPantryRepository : IFoodPantryRepository
{
    private readonly IConfiguration _configuration;

    public FoodPantryRepository(IConfiguration configuration)
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
        return await conn.QueryFirstOrDefaultAsync<DbFoodItem>(sql, new { id }, commandTimeout: 30);
    }

    public async Task<DbFoodItem?> GetByNameAsync(string name, CancellationToken ct)
    {
        await using var conn = new MySqlConnector.MySqlConnection(GetConnectionString());
        await conn.OpenAsync(ct);
        const string sql = "SELECT * FROM items WHERE Name = @name LIMIT 1";
        return await conn.QueryFirstOrDefaultAsync<DbFoodItem>(sql, new { name }, commandTimeout: 30);
    }
}

