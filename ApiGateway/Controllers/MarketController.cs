using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Grpc.Core;
using Market;

[ApiController]
[Route("api/market")]
public class MarketController : ControllerBase
{
    private readonly MarketService.MarketServiceClient _client;

    public MarketController(MarketService.MarketServiceClient client)
    {
        _client = client;
    }

    // GET: api/market/updates
    // Este endpoint mantiene la conexión abierta y envía datos en tiempo real al navegador
    [HttpGet("updates")]
    [Authorize(AuthenticationSchemes = "SupabaseAuth")] // Protegido
    public async Task GetMarketUpdates(CancellationToken cancellationToken)
    {
        Response.Headers.Add("Content-Type", "text/event-stream");

        // Pedimos actualizaciones cada 2 segundos
        var request = new MarketRequest { IntervalSeconds = 2 };

        using var call = _client.CheckMarket(request, cancellationToken: cancellationToken);

        try
        {
            await foreach (var response in call.ResponseStream.ReadAllAsync(cancellationToken))
            {
                // Enviamos cada tick al frontend como JSON
                var json = System.Text.Json.JsonSerializer.Serialize(response);
                await Response.WriteAsync($"data: {json}\n\n");
                await Response.Body.FlushAsync();
            }
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.Cancelled)
        {
            // El cliente cerró la conexión, normal.
        }
    }
}