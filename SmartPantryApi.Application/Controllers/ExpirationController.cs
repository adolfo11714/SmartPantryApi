using Microsoft.AspNetCore.Mvc;
using SmartPantryApi.Application.Services;

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
        var expiration = _foodKeeperService.CalculateExpiration(request.PurchasedOn, request.ProductName, request.StorageLocation);
        if (expiration is null)
        {
            return NotFound(new { message = "No duration found for product or storage location." });
        }

        return Ok(new ExpirationResponse
        {
            ProductName = request.ProductName,
            PurchasedOn = request.PurchasedOn,
            StorageLocation = request.StorageLocation,
            ExpiresOn = expiration.Value
        });
    }
}

public sealed class ExpirationRequest
{
    public string ProductName { get; set; } = string.Empty;
    public DateOnly PurchasedOn { get; set; }
    public StorageLocation StorageLocation { get; set; }
}

public sealed class ExpirationResponse
{
    public string ProductName { get; set; } = string.Empty;
    public DateOnly PurchasedOn { get; set; }
    public StorageLocation StorageLocation { get; set; }
    public DateOnly ExpiresOn { get; set; }
}


