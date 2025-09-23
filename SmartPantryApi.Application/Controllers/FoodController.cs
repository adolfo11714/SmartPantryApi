using Microsoft.AspNetCore.Mvc;
using SmartPantryApi.Application.Services;

namespace SmartPantryApi.Application.Controllers;

[ApiController]
[Route("api/food")]
public class FoodController : ControllerBase
{
    private readonly FoodKeeperService _foodKeeperService;
    private readonly ItemsRepository _itemsRepository;

    public FoodController(FoodKeeperService foodKeeperService, ItemsRepository itemsRepository)
    {
        _foodKeeperService = foodKeeperService;
        _itemsRepository = itemsRepository;
    }

    [HttpGet("{name}")]
    public ActionResult<FoodItemResponse> GetByName(string name)
    {
        var item = _foodKeeperService.GetItemByName(name);
        if (item is null)
        {
            return NotFound(new { message = "No food item found with that name." });
        }

        return Ok(new FoodItemResponse
        {
            Name = item.Name,
            PantryDays = item.PantryDays,
            RefrigeratorDays = item.RefrigeratorDays,
            FreezerDays = item.FreezerDays
        });
    }

    [HttpGet("db/id/{id:int}")]
    public async Task<IActionResult> GetDbById(int id, CancellationToken ct)
    {
        var item = await _itemsRepository.GetByIdAsync(id, ct);
        if (item is null) return NotFound();
        return Ok(item);
    }

    [HttpGet("db/name/{name}")]
    public async Task<IActionResult> GetDbByName(string name, CancellationToken ct)
    {
        var item = await _itemsRepository.GetByNameAsync(name, ct);
        if (item is null) return NotFound();
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


