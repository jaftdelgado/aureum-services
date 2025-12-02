using Grpc.Core;
using Market; // Namespace generado del proto
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

        // GET: api/market/status/{teamId}
        // Convierte el stream gRPC en una respuesta REST simple (Snapshot del mercado actual)
        [HttpGet("status/{teamId}")]
        public async Task<IActionResult> GetMarketStatus(string teamId)
        {
            try
            {
                // Solicitamos iniciar el stream
                using var call = _client.CheckMarket(new MarketRequest 
                { 
                    TeamPublicId = teamId,
                    IntervalSeconds = 1 // Pedimos intervalo corto para que responda rápido
                });

                // Esperamos solo el primer mensaje (Snapshot)
                if (await call.ResponseStream.MoveNext(CancellationToken.None))
                {
                    var data = call.ResponseStream.Current;
                    
                    // Mapeamos a un objeto anónimo (o DTO) para devolver JSON limpio
                    return Ok(new 
                    {
                        Timestamp = data.TimestampUnixMillis,
                        Assets = data.Assets.Select(a => new 
                        {
                            a.Id,
                            a.Symbol,
                            a.Name,
                            Price = Math.Round(a.Price, 2), // Redondear para frontend
                            a.BasePrice,
                            a.Volatility
                        })
                    });
                }
                
                return NotFound("No se recibieron datos del mercado.");
            }
            catch (RpcException ex)
            {
                return StatusCode(500, $"Error gRPC: {ex.Status.Detail}");
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