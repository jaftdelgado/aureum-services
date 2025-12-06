using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using Microsoft.Extensions.Caching.Memory;
using Grpc.Core;
using System.Security.Claims;

AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

var marketUrl = Environment.GetEnvironmentVariable("MARKET_SERVICE_URL") ?? "http://marketservice.railway.internal:50051";
var lessonsUrl = Environment.GetEnvironmentVariable("LESSONS_SERVICE_URL") ?? "http://lessonsservice.railway.internal:50051";
var userServiceUrl = Environment.GetEnvironmentVariable("USER_SERVICE_URL") ?? "http://aureum-services.railway.internal:8001";
var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

var supabaseSecret = Environment.GetEnvironmentVariable("SUPABASE_JWT_SECRET") ?? "CLAVE_TEMPORAL_PARA_LOCAL_123456789";
var bytes = Encoding.UTF8.GetBytes(supabaseSecret);

builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirTodo", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

builder.Services.AddMemoryCache();
builder.Services.AddHttpClient("UserServiceClient", client =>
{
    client.BaseAddress = new Uri(userServiceUrl);
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
        OnTokenValidated = async context =>
        {
            var identity = context.Principal.Identity as ClaimsIdentity;
            var userId = identity?.FindFirst("sub")?.Value;

            if (!string.IsNullOrEmpty(userId))
            {
                var cache = context.HttpContext.RequestServices.GetRequiredService<IMemoryCache>();
                var httpClientFactory = context.HttpContext.RequestServices.GetRequiredService<IHttpClientFactory>();

                var cacheKey = $"user_role_{userId}";

                if (!cache.TryGetValue(cacheKey, out string role))
                {
                    try
                    {
                        var client = httpClientFactory.CreateClient("UserServiceClient");

                        var response = await client.GetAsync($"/api/users/profiles/{userId}");

                        if (response.IsSuccessStatusCode)
                        {
                            var content = await response.Content.ReadAsStringAsync();
                            using var doc = JsonDocument.Parse(content);

                            if (doc.RootElement.TryGetProperty("role", out var roleElement))
                            {
                                role = roleElement.GetString();

                                var cacheEntryOptions = new MemoryCacheEntryOptions()
                                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(5));

                                cache.Set(cacheKey, role, cacheEntryOptions);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error obteniendo rol para {userId}: {ex.Message}");
                    }
                }

                if (!string.IsNullOrEmpty(role))
                {
                    identity.AddClaim(new Claim("user_role", role));

                    identity.AddClaim(new Claim(ClaimTypes.Role, role));
                }
            }
        }
    };
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
        OnTokenValidated = context =>
        {
            var identity = context.Principal.Identity as ClaimsIdentity;
            var userMetadataClaim = identity?.FindFirst("user_metadata");

            if (userMetadataClaim != null)
            {
                try
                {
                    using var doc = JsonDocument.Parse(userMetadataClaim.Value);
                    var root = doc.RootElement;

                    if (root.TryGetProperty("role", out var roleElement))
                    {
                        var roleValue = roleElement.GetString();
                        if (!string.IsNullOrEmpty(roleValue))
                        {
                            identity.AddClaim(new Claim("user_role", roleValue));
                        }
                    }
                }
                catch
                {
                }
            }

            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ProfessorOnly", policy =>
        policy.RequireClaim("user_role", "professor", "teacher"));

    options.AddPolicy("StudentOnly", policy =>
        policy.RequireClaim("user_role", "student"));

    options.AddPolicy("AuthenticatedUser", policy =>
        policy.RequireAuthenticatedUser());
});

builder.Services.AddControllers();

builder.Services.AddGrpcClient<Market.MarketService.MarketServiceClient>(o =>
{
    o.Address = new Uri(marketUrl);
})
.ConfigureChannel(o =>
{
    var handler = new SocketsHttpHandler
    {
        PooledConnectionIdleTimeout = Timeout.InfiniteTimeSpan,
        KeepAlivePingDelay = TimeSpan.FromSeconds(60),
        KeepAlivePingTimeout = TimeSpan.FromSeconds(30),
        EnableMultipleHttp2Connections = true
    };
    o.HttpHandler = handler;
});

// --- INICIO DE LA CORRECCIÓN ---
builder.Services.AddGrpcClient<Trading.LeccionesService.LeccionesServiceClient>(o =>
{
    o.Address = new Uri(lessonsUrl); 
})
.ConfigureChannel(options =>
{
    options.MaxReceiveMessageSize = null; 
})
.ConfigurePrimaryHttpMessageHandler(() =>
{
   
    var handler = new HttpClientHandler();
    handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
    return handler;
})
.ConfigureHttpClient(client =>
{
    client.DefaultRequestVersion = new Version(2, 0);
    client.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrHigher;
    client.Timeout = Timeout.InfiniteTimeSpan;
});

builder.Services.AddOcelot();

var app = builder.Build();

app.UseCors("PermitirTodo");

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

await app.UseOcelot();

app.Run();
