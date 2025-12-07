using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortfolioService.Data;
using PortfolioService.Models;
using PortfolioService.Dtos;

namespace PortfolioService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PortfolioController : ControllerBase
    {
        private readonly PortfolioContext _portfolioContext;
        private readonly MarketContext _marketContext;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public PortfolioController(PortfolioContext portfolioContext, MarketContext marketContext, IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _portfolioContext = portfolioContext;
            _marketContext = marketContext;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }
        private string GetAssetServiceUrl()
        {
         
            var url = _configuration["AssetServiceUrl"] ?? "http://assetservice.railway.internal:3000";
            return url.TrimEnd('/');
        }

        [HttpGet("course/{courseId}")]
        public async Task<ActionResult<IEnumerable<PortfolioDto>>> GetByCourse(Guid courseId)
        {
            var items = await _portfolioContext.PortfolioItems
                                     .Where(p => p.TeamId == courseId && p.IsActive)
                                     .ToListAsync();

            if (items == null || !items.Any())
            {
                return Ok(new List<PortfolioDto>());
            }

            var result = new List<PortfolioDto>();
            var client = _httpClientFactory.CreateClient();
            var baseUrl = GetAssetServiceUrl();

            foreach (var item in items)
            {
                string name = "Cargando...";
                string symbol = "---";

                try
                {
                    var response = await client.GetFromJsonAsync<AssetExternalDto>(
                        $"{baseUrl}/assets/{item.AssetId}");

                    if (response != null)
                    {
                        name = response.Name;
                        symbol = response.Symbol;
                    }
                }
                catch
                {
                    Console.WriteLine($"Error conectando a AssetService para {item.AssetId}");
                }

                double currentTotal = item.Quantity * item.CurrentValue;
                double investedTotal = item.Quantity * item.AvgPrice;
                double pnl = currentTotal - investedTotal;
                double pnlPct = investedTotal != 0 ? (pnl / investedTotal) * 100 : 0;

                result.Add(new PortfolioDto
                {
                    UserId = item.UserId,
                    PortfolioId = item.PortfolioId,
                    AssetId = item.AssetId,
                    Quantity = item.Quantity,
                    AvgPrice = item.AvgPrice,
                    CurrentValue = item.CurrentValue,
                    AssetName = name,
                    AssetSymbol = symbol,
                    TotalInvestment = investedTotal,
                    CurrentTotalValue = currentTotal,
                    ProfitOrLoss = pnl,
                    ProfitOrLossPercentage = Math.Round(pnlPct, 2)
                });
            }

            return Ok(result);
        }

        [HttpGet("detail/{portfolioId}")]
        public async Task<ActionResult<PortfolioDto>> GetPortfolioDetail(int portfolioId)
        {
            var item = await _portfolioContext.PortfolioItems.FindAsync(portfolioId);

            if (item == null) return NotFound("El activo no existe en el portafolio.");

            var client = _httpClientFactory.CreateClient();
            var baseUrl = GetAssetServiceUrl();
            string name = "Desconocido";
            string symbol = "---";

            try
            {
                var response = await client.GetFromJsonAsync<AssetExternalDto>(
                    $"{baseUrl}/assets/{item.AssetId}");

                if (response != null)
                {
                    name = response.Name;
                    symbol = response.Symbol;
                }
            }
            catch { }

            double currentTotal = item.Quantity * item.CurrentValue;
            double investedTotal = item.Quantity * item.AvgPrice;
            double pnl = currentTotal - investedTotal;
            double pnlPct = investedTotal != 0 ? (pnl / investedTotal) * 100 : 0;

            var detail = new PortfolioDto
            {
                UserId = item.UserId,
                PortfolioId = item.PortfolioId,
                AssetId = item.AssetId,
                Quantity = item.Quantity,
                AvgPrice = item.AvgPrice,
                CurrentValue = item.CurrentValue,
                AssetName = name,
                AssetSymbol = symbol,
                TotalInvestment = investedTotal,
                CurrentTotalValue = currentTotal,
                ProfitOrLoss = pnl,
                ProfitOrLossPercentage = Math.Round(pnlPct, 2)
            };

            return Ok(detail);
        }

        [HttpGet("history/course/{courseId}/student/{studentId}")]
        public async Task<ActionResult<IEnumerable<HistoryDto>>> GetHistory(Guid courseId, Guid studentId)
        {
           
            var movements = await _marketContext.Movements
                                    .Include(m => m.Transaction)
                                   .Where(m => m.UserId == studentId && m.PublicId == courseId)
                                    .OrderByDescending(m => m.CreatedDate)
                                    .ToListAsync();

            if (movements == null || !movements.Any())
            {
                
                return Ok(new List<HistoryDto>());
            }

            var resultList = new List<HistoryDto>();
            var client = _httpClientFactory.CreateClient();
            var baseUrl = GetAssetServiceUrl(); 



            foreach (var mov in movements)
            {
                
                string name = "Desconocido";
                string symbol = "---";

                try
                {
                    var url = $"{baseUrl}/assets/{mov.AssetId}";
                    var response = await client.GetFromJsonAsync<AssetExternalDto>(url);

                    if (response != null)
                    {
                        
                        name = response.Name ?? "Sin Nombre";
                        symbol = response.Symbol ?? "---";
                    }
                }
                catch (Exception ex)
                {
                    
                    Console.WriteLine($"Error en historial para Asset {mov.AssetId}: {ex.Message}");
                }

                decimal price = mov.Transaction?.TransactionPrice ?? 0;
                bool isBuy = mov.Transaction?.IsBuy ?? false;

                resultList.Add(new HistoryDto
                {
                    MovementId = mov.PublicId,
                    AssetId = mov.AssetId,

                    
                    AssetName = name,
                    AssetSymbol = symbol,

                    Quantity = mov.Quantity,
                    Price = price,
                    TotalAmount = mov.Quantity * price,
                    Type = isBuy ? "Compra" : "Venta",
                    Date = mov.CreatedDate
                });
            }

            return Ok(resultList);
        }


        [HttpPost("transaction")] 
        public async Task<IActionResult> RegisterSmartTransaction([FromBody] PortfolioTransactionDto dto)
        {
           if (dto.UserId == Guid.Empty || dto.TeamId == Guid.Empty || dto.AssetId == Guid.Empty)
    {
        return BadRequest(new { message = "Los IDs no pueden estar vacíos. Verifica los nombres de las propiedades en tu JSON." });
    }
            var item = await _portfolioContext.PortfolioItems
                                .FirstOrDefaultAsync(p => p.UserId == dto.UserId
                                                       && p.AssetId == dto.AssetId
                                                       && p.TeamId == dto.TeamId);

            
            if (item == null)
            {
                if (!dto.IsBuy)
                {
                    return BadRequest(new { message = "No puedes vender un activo que no tienes." });
                }

                item = new PortfolioItem
                {
                    
                    PublicId = Guid.NewGuid(),
                    UserId = dto.UserId,
                    TeamId = dto.TeamId,
                    AssetId = dto.AssetId,
                    Quantity = dto.Quantity,
                    AvgPrice = (double)dto.Price, 
                    CurrentValue = (double)dto.Price, 
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    Notes = "Primera compra"
                };

                _portfolioContext.PortfolioItems.Add(item);
            }
            
            else
            {
                if (dto.IsBuy)
                {
                   
                    double currentTotalCost = item.Quantity * item.AvgPrice;
                    double newPurchaseCost = dto.Quantity * (double)dto.Price;
                    double finalQuantity = item.Quantity + dto.Quantity;

                    if (finalQuantity > 0)
                    {
                        item.AvgPrice = (currentTotalCost + newPurchaseCost) / finalQuantity;
                    }
                    item.Quantity = finalQuantity;
                    item.IsActive = true; 
                }
                else
                {
                 
                    if (item.Quantity < dto.Quantity)
                        return BadRequest(new { message = "Fondos insuficientes." });

                    item.Quantity -= dto.Quantity;

                    if (item.Quantity <= 0.000001)
                    {
                        item.Quantity = 0;
                        item.IsActive = false; 
                    }
                }
                item.UpdatedAt = DateTime.UtcNow;
            }

            
            var movement = new Movement
            {
                PublicId = dto.TeamId,
                UserId = dto.UserId,
                AssetId = dto.AssetId,
                Quantity = (decimal)dto.Quantity,
                CreatedDate = DateTime.UtcNow,
                Transaction = new Transaction
                {
                    PublicId = Guid.NewGuid(),
                    TransactionPrice = dto.Price,
                    IsBuy = dto.IsBuy,
                    CreatedDate = DateTime.UtcNow
                }
            };

            _marketContext.Movements.Add(movement);

          
            await _portfolioContext.SaveChangesAsync();
            await _marketContext.SaveChangesAsync();

            return Ok(new { message = "Transacción exitosa", currentQuantity = item.Quantity });
        }

        [HttpDelete("{portfolioId}")]
        public async Task<IActionResult> DeletePortfolioItem(int portfolioId)
        {
            var item = await _portfolioContext.PortfolioItems.FindAsync(portfolioId);

            if (item == null)
            {
                return NotFound(new { message = "El activo no existe." });
            }

            
            _portfolioContext.PortfolioItems.Remove(item);

            await _portfolioContext.SaveChangesAsync();

            return Ok(new { message = "Activo eliminado permanentemente del portafolio." });
        }

        [HttpPost]
        public async Task<ActionResult<PortfolioItem>> PostPortfolioItem(PortfolioItem item)
        {
            if (item.UserId == Guid.Empty || item.AssetId == Guid.Empty || item.TeamId == Guid.Empty)
            {
                return BadRequest("UserId, AssetId y TeamId son obligatorios.");
            }

            if (item.PublicId == Guid.Empty) item.PublicId = Guid.NewGuid();

            item.IsActive = true;
            item.CreatedAt = DateTime.UtcNow;
            item.UpdatedAt = DateTime.UtcNow;

            _portfolioContext.PortfolioItems.Add(item);
            await _portfolioContext.SaveChangesAsync();

            return CreatedAtAction(nameof(GetByCourse), new { courseId = item.TeamId }, item);
        }
    }
}
