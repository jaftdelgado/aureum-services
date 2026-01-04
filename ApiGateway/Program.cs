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
using ApiGateway.Services;          
using ApiGateway.Services.External;

JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

var builder = WebApplication.CreateBuilder(args);

var marketUrl = Environment.GetEnvironmentVariable("MARKET_SERVICE_URL") ?? "http://marketservice.railway.internal:50051";
var lessonsUrl = Environment.GetEnvironmentVariable("LESSONS_SERVICE_URL") ?? "http://lessonsservice.railway.internal:50051";
var userServiceUrl = Environment.GetEnvironmentVariable("USER_SERVICE_URL") ?? "http://aureum-services.railway.internal:8001";
var supabaseSecret = Environment.GetEnvironmentVariable("SUPABASE_JWT_SECRET") ?? "CLAVE_TEMPORAL_PARA_LOCAL_123456789";

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
        OnMessageReceived = context =>
    {
        var accessToken = context.Request.Query["token"];
        if (!string.IsNullOrEmpty(accessToken))
        {
            context.Token = accessToken;
        }
        return Task.CompletedTask;
    },
        OnAuthenticationFailed = context =>
        {
            return Task.CompletedTask;
        },
        OnTokenValidated = async context =>
        {
            
            var identity = context.Principal?.Identity as ClaimsIdentity;

            var userId = identity?.FindFirst("sub")?.Value 
                      ?? identity?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!string.IsNullOrEmpty(userId))
            {
                
                string role = "student"; 
                
                try 
                {
                    using var client = new HttpClient();
                    client.BaseAddress = new Uri(userServiceUrl);
                    client.Timeout = TimeSpan.FromSeconds(2); 
                    
                    var response = await client.GetAsync($"/api/v1/profiles/{userId}");
                    
                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        using var doc = JsonDocument.Parse(content);
                        if (doc.RootElement.TryGetProperty("role", out var r))
                        {
                            role = r.GetString() ?? "student";
                        }
                    }
                    else
                    {
                    }
                }
                catch (Exception ex)
                {
                }

                if (identity != null)
                {
                    identity.AddClaim(new Claim("user_role", role)); 
                    identity.AddClaim(new Claim(ClaimTypes.Role, role));
                    
                }
            }
            else
            {
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
builder.Services.AddScoped<ILessonsGateway, LessonsGrpcGateway>();
builder.Services.AddScoped<ILessonsService, LessonsService>();

builder.Services.AddOcelot();

var app = builder.Build();

app.UseCors("PermitirTodo");
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.MapWhen(ctx =>
       ctx.Request.Path.StartsWithSegments("/api/users")
    || ctx.Request.Path.StartsWithSegments("/api/assets")
    || ctx.Request.Path.StartsWithSegments("/api/team-assets")
    || ctx.Request.Path.StartsWithSegments("/api/portfolio")
    || ctx.Request.Path.StartsWithSegments("/api/courses")
    || ctx.Request.Path.StartsWithSegments("/api/memberships")
    || ctx.Request.Path.StartsWithSegments("/api/market-config")
    || ctx.Request.Path.StartsWithSegments("/api/cloud"),
    ocelotBranch =>
    {
        ocelotBranch.UseOcelot().Wait();
    });

app.Run();
