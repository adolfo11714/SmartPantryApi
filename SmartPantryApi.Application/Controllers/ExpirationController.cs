using Microsoft.AspNetCore.Mvc;
using SmartPantryApi.Application.Services;
using SmartyPantryApi.Application.Domain.Enums;

namespace SmartPantryApi.Application.Controllers;

[ApiController]
[Route("api/expiration")]
public class ExpirationController : ControllerBase
{
    private readonly FoodKeeperService _foodKeeperService;

    public ExpirationController(FoodKeeperService foodKeeperService)
    {
        _foodKeeperService = foodKeeperService;
    }

    [HttpPost]
    public ActionResult<ExpirationResponse> Calculate([FromBody] ExpirationRequest request)
    {
        var expiration = _foodKeeperService.CalculateExpiration(request.PurchasedOn, request.ProductName, request.StorageLocationEnum);
        if (expiration is null)
        {
            return NotFound(new { message = "No duration found for product or storage location." });
        }

        return Ok(new ExpirationResponse
        {
            ProductName = request.ProductName,
            PurchasedOn = request.PurchasedOn,
            StorageLocationEnum = request.StorageLocationEnum,
            ExpiresOn = expiration.Value
        });
    }
}

public sealed class ExpirationRequest
{
    public string ProductName { get; set; } = string.Empty;
    public DateOnly PurchasedOn { get; set; }
    public StorageLocationEnum StorageLocationEnum { get; set; }
}

public sealed class ExpirationResponse
{
    public string ProductName { get; set; } = string.Empty;
    public DateOnly PurchasedOn { get; set; }
    public StorageLocationEnum StorageLocationEnum { get; set; }
    public DateOnly ExpiresOn { get; set; }
}


