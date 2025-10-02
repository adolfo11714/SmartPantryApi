using Microsoft.AspNetCore.Mvc;
using SmartPantryApi.Application.Infrastructure;
using SmartPantryApi.Application.Services;
using SmartPantryApi.Application.Services.Interfaces;

namespace SmartPantryApi.Application.Controllers;

[ApiController]
[Route("api/food")]
public class FoodController : ControllerBase
{
    
    private readonly IFoodPantryService _foodPantryService;

    public FoodController(IFoodPantryService foodPantryService)
    {
        _foodPantryService = foodPantryService;
    }

    [HttpGet("db/id/{id:int}")]
    public async Task<IActionResult> GetDbById(int id, CancellationToken ct)
    {
        var item = await _foodPantryService.GetByIdAsync(id, ct);
        if (item is null) return NotFound();
        return Ok(item);
    }

    [HttpGet("db/name/{name}")]
    public async Task<IActionResult> GetDbByName(string name, CancellationToken ct)
    {
        var item = await _foodPantryService.GetByNameAsync(name, ct);
        if (item is null) return NotFound();
        return Ok(item);
    }

    [HttpGet("db/category_id/{id}")]
    public async Task<IActionResult> GetCategoryDbById(int id, CancellationToken ct)
    {
        var item = await _foodPantryService.GetCategoryByIdAsync(id, ct);
        if (item.ID == 0) return NotFound();
        return Ok(item);
    }
}

public sealed class FoodItemResponse
{
    public string Name { get; set; } = string.Empty;
    public int? PantryDays { get; set; }
    public int? RefrigeratorDays { get; set; }
    public int? FreezerDays { get; set; }
}


