using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using Grpc.Core;

AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

var marketUrl = Environment.GetEnvironmentVariable("MARKET_SERVICE_URL") ?? "http://marketservice.railway.internal:50051";
var lessonsUrl = Environment.GetEnvironmentVariable("LESSONS_SERVICE_URL") ?? "http://lessonsservice.railway.internal:50051";

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
    var handler = new SocketsHttpHandler
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
