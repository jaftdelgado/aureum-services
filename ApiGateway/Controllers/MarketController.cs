using Grpc.Core;
using Market; // Namespace generado del proto
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace ApiGateway.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MarketController : ControllerBase
    {
        private readonly MarketService.MarketServiceClient _client;

        public MarketController(MarketService.MarketServiceClient client)
        {
            _client = client;
        }

        [HttpGet("stream/{teamId}")]
        public async Task GetMarketStream(string teamId, CancellationToken cancellationToken)
        {

            Response.Headers.Append("Content-Type", "text/event-stream");
            Response.Headers.Append("Cache-Control", "no-cache");
            Response.Headers.Append("Connection", "keep-alive");

            try
            {
                using var call = _client.CheckMarket(new MarketRequest 
                { 
                    TeamPublicId = teamId,
                    IntervalSeconds = 4 
                }, cancellationToken: cancellationToken);

                while (await call.ResponseStream.MoveNext(cancellationToken))
                {
                    var data = call.ResponseStream.Current;

                    var payload = new 
                    {
                        Timestamp = data.TimestampUnixMillis,
                        Assets = data.Assets.Select(a => new 
                        {
                            a.Id,
                            a.Symbol,
                            a.Name,
                            Price = Math.Round(a.Price, 2),
                            a.BasePrice,
                            a.Volatility
                        })
                    };

                    var json = JsonSerializer.Serialize(payload);
                    await Response.WriteAsync($"data: {json}\n\n", cancellationToken);
                    await Response.Body.FlushAsync(cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (RpcException ex)
            {
                 await Response.WriteAsync($"event: error\ndata: {ex.Status.Detail}\n\n", cancellationToken);
            }
        }
        
        // POST: api/market/buy
        [HttpPost("buy")]
        [Authorize] // Asumiendo que requieres login
        public async Task<IActionResult> BuyAsset([FromBody] BuyAssetRequest request)
        {
            try
            {
                // El Gateway pasa la petición al microservicio tal cual
                var response = await _client.BuyAssetAsync(request);
                return Ok(response);
            }
            catch (RpcException ex)
            {
                return StatusCode(500, $"Error en compra: {ex.Status.Detail}");
            }
        }

        // POST: api/market/sell
        [HttpPost("sell")]
        [Authorize]
        public async Task<IActionResult> SellAsset([FromBody] SellAssetRequest request)
        {
            try
            {
                var response = await _client.SellAssetAsync(request);
                return Ok(response);
            }
            catch (RpcException ex)
            {
                return StatusCode(500, $"Error en venta: {ex.Status.Detail}");
            }
        }
    }
}