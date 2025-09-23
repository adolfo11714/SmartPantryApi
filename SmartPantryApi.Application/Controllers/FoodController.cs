using Microsoft.AspNetCore.Mvc;
using SmartPantryApi.Application.Services;

namespace SmartPantryApi.Application.Controllers;

[ApiController]
[Route("api/food")]
public class FoodController : ControllerBase
{
    private readonly FoodKeeperService _foodKeeperService;

    public FoodController(FoodKeeperService foodKeeperService)
    {
        _foodKeeperService = foodKeeperService;
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
}

public sealed class FoodItemResponse
{
    public string Name { get; set; } = string.Empty;
    public int? PantryDays { get; set; }
    public int? RefrigeratorDays { get; set; }
    public int? FreezerDays { get; set; }
}


