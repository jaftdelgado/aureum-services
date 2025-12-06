using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using Grpc.Core;
using System.Security.Claims;
using System.Text.Json;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Caching.Memory;

JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

var builder = WebApplication.CreateBuilder(args);

var marketUrl = Environment.GetEnvironmentVariable("MARKET_SERVICE_URL") ?? "http://marketservice.railway.internal:50051";
var lessonsUrl = Environment.GetEnvironmentVariable("LESSONS_SERVICE_URL") ?? "http://lessonsservice.railway.internal:50051";
var userServiceUrl = Environment.GetEnvironmentVariable("USER_SERVICE_URL") ?? "http://aureum-services.railway.internal:8001";
var supabaseSecret = Environment.GetEnvironmentVariable("SUPABASE_JWT_SECRET") ?? "CLAVE_TEMPORAL_PARA_LOCAL_123456789";

Console.WriteLine($"[Gateway Config] UserService URL: {userServiceUrl}");

builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);
builder.Logging.SetMinimumLevel(LogLevel.Debug);
builder.Logging.AddConsole();
var bytes = Encoding.UTF8.GetBytes(supabaseSecret);

builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirTodo", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

builder.Services.AddControllers();
builder.Services.AddMemoryCache();

builder.Services.AddHttpClient("UserServiceClient", client =>
{
    client.BaseAddress = new Uri(userServiceUrl);
    client.Timeout = TimeSpan.FromSeconds(5);
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = "SupabaseAuth";
    options.DefaultChallengeScheme = "SupabaseAuth";
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

    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine($"[Auth Error] Token inválido: {context.Exception.Message}");
            return Task.CompletedTask;
        },
        OnTokenValidated = async context =>
        {
            Console.WriteLine(">>> [DEBUG] OnTokenValidated EJECUTADO <<<");
            
            var identity = context.Principal?.Identity as ClaimsIdentity;

            var userId = identity?.FindFirst("sub")?.Value 
                      ?? identity?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!string.IsNullOrEmpty(userId))
            {
                Console.WriteLine($">>> [DEBUG] Usuario autenticado: {userId}");
                
                
                string role = "student"; 
                
                try 
                {
                    using var client = new HttpClient();
                    client.BaseAddress = new Uri(userServiceUrl);
                    client.Timeout = TimeSpan.FromSeconds(2); 
                    
                    Console.WriteLine($">>> [DEBUG] Consultando rol en: {userServiceUrl}/api/v1/profiles/{userId}");
                    var response = await client.GetAsync($"/api/v1/profiles/{userId}");
                    
                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        using var doc = JsonDocument.Parse(content);
                        if (doc.RootElement.TryGetProperty("role", out var r))
                        {
                            role = r.GetString() ?? "student";
                            Console.WriteLine($">>> [DEBUG] ROL REAL OBTENIDO: {role}");
                        }
                    }
                    else
                    {
                        Console.WriteLine($">>> [DEBUG] Error HTTP {response.StatusCode} del UserService");
                    }
                }
                catch (Exception ex)
                {
                     Console.WriteLine($">>> [DEBUG] Excepción conectando: {ex.Message}");
                }

                if (identity != null)
                {
                    identity.AddClaim(new Claim("user_role", role)); 
                    identity.AddClaim(new Claim(ClaimTypes.Role, role));
                    
                    Console.WriteLine($">>> [DEBUG] Claims inyectados: user_role={role}");
                }
            }
            else
            {
                Console.WriteLine(">>> [DEBUG] No se encontró 'sub' en el token.");
            }
        }
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ProfessorOnly", policy => policy.RequireClaim("user_role", "professor", "teacher"));
    options.AddPolicy("StudentOnly", policy => policy.RequireClaim("user_role", "student"));
});

HttpMessageHandler CreateGrpcHandler()
{
    var handler = new SocketsHttpHandler
    {
        PooledConnectionIdleTimeout = Timeout.InfiniteTimeSpan,
        KeepAlivePingDelay = TimeSpan.FromSeconds(60),
        KeepAlivePingTimeout = TimeSpan.FromSeconds(30),
        EnableMultipleHttp2Connections = true
    };
    return handler;
}

builder.Services.AddGrpcClient<Market.MarketService.MarketServiceClient>(o =>
{
    o.Address = new Uri(marketUrl);
})
.ConfigurePrimaryHttpMessageHandler(CreateGrpcHandler);

builder.Services.AddGrpcClient<Trading.LeccionesService.LeccionesServiceClient>(o =>
{
    o.Address = new Uri(lessonsUrl);
})
.ConfigurePrimaryHttpMessageHandler(CreateGrpcHandler)
.ConfigureChannel(options => options.MaxReceiveMessageSize = null);

builder.Services.AddOcelot();

var app = builder.Build();

app.UseCors("PermitirTodo");
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

await app.UseOcelot();

app.Run();
