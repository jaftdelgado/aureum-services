using Microsoft.EntityFrameworkCore;
using PortfolioService.Data;

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

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();