using Microsoft.EntityFrameworkCore;
using PortfolioService.Data;
using PortfolioService.Services;
using PortfolioService.Services.External;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<PortfolioContext>(options =>
    options.UseNpgsql(connectionString));

var marketConnectionString = builder.Configuration.GetConnectionString("MarketConnection");
builder.Services.AddDbContext<MarketContext>(options =>
    options.UseNpgsql(marketConnectionString));

builder.Services.AddHttpClient();
builder.Services.AddScoped<IAssetGateway, AssetGateway>();
builder.Services.AddScoped<IPortfolioManagementService, PortfolioManagementService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

app.Run();
public partial class Program { }