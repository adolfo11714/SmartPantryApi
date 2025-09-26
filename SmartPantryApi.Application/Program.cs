using SmartPantryApi.Application.Infrastructure;
using SmartPantryApi.Application.Infrastructure.Interfaces;
using SmartPantryApi.Application.Services;
using SmartPantryApi.Application.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// SmartPantry services
builder.Services.AddSingleton<FoodKeeperService>();
builder.Services.AddSingleton<System.Data.Common.DbProviderFactory>(sp => MySqlConnector.MySqlConnectorFactory.Instance);
builder.Services.AddSingleton<IFoodPantryRepository, FoodPantryRepository>();
builder.Services.AddSingleton<IFoodPantryService, FoodPantryService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthorization();

app.MapControllers();

app.Run();