using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

builder.Services.AddControllers();

var supabaseSecret = Environment.GetEnvironmentVariable("SUPABASE_JWT_SECRET") ?? "CLAVE_TEMPORAL_PARA_LOCAL_123456789";
var bytes = Encoding.UTF8.GetBytes(supabaseSecret);

builder.Services.AddGrpcClient<Market.MarketService.MarketServiceClient>(o =>
{
    o.Address = new Uri("http://marketservice.railway.internal:50051");
});

builder.Services.AddGrpcClient<Trading.LeccionesService.LeccionesServiceClient>(o =>
{
    o.Address = new Uri("http://lessonsservice.railway.internal:50051");
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirTodo", policy =>
    {
        policy.AllowAnyOrigin() 
              .AllowAnyMethod()  
              .AllowAnyHeader(); 
    });
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer("SupabaseAuth", options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(bytes),
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddOcelot();

var app = builder.Build();

app.UseCors("PermitirTodo");
app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

await app.UseOcelot();  

app.Run();
