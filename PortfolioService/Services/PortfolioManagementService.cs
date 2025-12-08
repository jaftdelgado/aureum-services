using Microsoft.EntityFrameworkCore;
using PortfolioService.Data;
using PortfolioService.Dtos;
using PortfolioService.Models;
using PortfolioService.Services.External;


namespace PortfolioService.Services
{
    public interface IPortfolioManagementService
    {
        Task<IEnumerable<PortfolioDto>> GetPortfolioByCourseAsync(Guid courseId);
        Task<IEnumerable<HistoryDto>> GetHistoryAsync(Guid courseId, Guid studentId);
        Task<PortfolioDto?> GetPortfolioDetailAsync(int portfolioId);
        Task<(bool Success, string Message, double CurrentQuantity)> ProcessTransactionAsync(PortfolioTransactionDto dto);
        Task<(bool Success, string Message)> DeletePortfolioItemAsync(int portfolioId);
        Task<PortfolioItem> CreatePortfolioItemAsync(PortfolioItem item);
        Task<(bool Success, string Message, Guid? WalletId, decimal? Balance)> UpdateWalletAsync(WalletDto dto);
    }

    public class PortfolioManagementService : IPortfolioManagementService
    {
        private readonly PortfolioContext _portfolioContext;
        private readonly MarketContext _marketContext;
        private readonly IAssetGateway _assetGateway;

        public PortfolioManagementService(
            PortfolioContext portfolioContext,
            MarketContext marketContext,
            IAssetGateway assetGateway)
        {
            _portfolioContext = portfolioContext;
            _marketContext = marketContext;
            _assetGateway = assetGateway;
        }
        public async Task<IEnumerable<PortfolioDto>> GetPortfolioByCourseAsync(Guid courseId)
        {
            var items = await _portfolioContext.PortfolioItems
                                     .Where(p => p.TeamId == courseId && p.IsActive)
                                     .ToListAsync();

            if (!items.Any()) return new List<PortfolioDto>();

            var tasks = items.Select(async item =>
            {
                var assetInfo = await _assetGateway.GetAssetInfoAsync(item.AssetId);

                double currentTotal = item.Quantity * item.CurrentValue;
                double investedTotal = item.Quantity * item.AvgPrice;
                double pnl = currentTotal - investedTotal;
                double pnlPct = investedTotal != 0 ? (pnl / investedTotal) * 100 : 0;

                return new PortfolioDto
                {
                    UserId = item.UserId,
                    PortfolioId = item.PortfolioId,
                    AssetId = item.AssetId,
                    Quantity = item.Quantity,
                    AvgPrice = item.AvgPrice,
                    CurrentValue = item.CurrentValue,
                    AssetName = assetInfo.Name,
                    AssetSymbol = assetInfo.Symbol,
                    TotalInvestment = investedTotal,
                    CurrentTotalValue = currentTotal,
                    ProfitOrLoss = pnl,
                    ProfitOrLossPercentage = Math.Round(pnlPct, 2)
                };
            });

            return await Task.WhenAll(tasks);
        }
        public async Task<PortfolioDto?> GetPortfolioDetailAsync(int portfolioId)
        {
            var item = await _portfolioContext.PortfolioItems.FindAsync(portfolioId);
            if (item == null) return null;

            var assetInfo = await _assetGateway.GetAssetInfoAsync(item.AssetId);

            double currentTotal = item.Quantity * item.CurrentValue;
            double investedTotal = item.Quantity * item.AvgPrice;
            double pnl = currentTotal - investedTotal;
            double pnlPct = investedTotal != 0 ? (pnl / investedTotal) * 100 : 0;

            return new PortfolioDto
            {
                UserId = item.UserId,
                PortfolioId = item.PortfolioId,
                AssetId = item.AssetId,
                Quantity = item.Quantity,
                AvgPrice = item.AvgPrice,
                CurrentValue = item.CurrentValue,
                AssetName = assetInfo.Name,
                AssetSymbol = assetInfo.Symbol,
                TotalInvestment = investedTotal,
                CurrentTotalValue = currentTotal,
                ProfitOrLoss = pnl,
                ProfitOrLossPercentage = Math.Round(pnlPct, 2)
            };
        }
        public async Task<IEnumerable<HistoryDto>> GetHistoryAsync(Guid courseId, Guid studentId)
        {
            var movements = await _marketContext.Movements
                                    .Include(m => m.Transaction)
                                    .Where(m => m.UserId == studentId && m.TeamId == courseId)
                                    .OrderByDescending(m => m.CreatedDate)
                                    .ToListAsync();

            if (!movements.Any()) return new List<HistoryDto>();

            var tasks = movements.Select(async mov =>
            {
                var assetInfo = await _assetGateway.GetAssetInfoAsync(mov.AssetId);

                decimal price = mov.Transaction?.TransactionPrice ?? 0;
                bool isBuy = mov.Transaction?.IsBuy ?? false;
                decimal pnl = mov.Transaction?.RealizedPnl ?? 0;

                return new HistoryDto
                {
                    MovementId = mov.PublicId,
                    AssetId = mov.AssetId,
                    AssetName = assetInfo.Name,
                    AssetSymbol = assetInfo.Symbol,
                    Quantity = mov.Quantity,
                    Price = price,
                    TotalAmount = mov.Quantity * price,
                    Type = isBuy ? "Compra" : "Venta",
                    RealizedPnl = pnl,
                    Date = mov.CreatedDate
                };
            });

            return await Task.WhenAll(tasks);
        }

        public async Task<(bool Success, string Message, double CurrentQuantity)> ProcessTransactionAsync(PortfolioTransactionDto dto)
        {
            var item = await _portfolioContext.PortfolioItems
                                .FirstOrDefaultAsync(p => p.UserId == dto.UserId
                                                       && p.AssetId == dto.AssetId
                                                       && p.TeamId == dto.TeamId);
            decimal realizedPnl = 0;

            if (item == null)
            {
                if (!dto.IsBuy)
                {
                    return (false, "No puedes vender un activo que no tienes.", 0);
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
                    realizedPnl = 0;
                }
                else
                {
                    if (item.Quantity < dto.Quantity)
                        return (false, "Fondos insuficientes.", item.Quantity);

                    decimal avgPriceDecimal = (decimal)item.AvgPrice;
                    realizedPnl = (dto.Price - avgPriceDecimal) * (decimal)dto.Quantity;
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
                PublicId = Guid.NewGuid(),
                TeamId = dto.TeamId,
                UserId = dto.UserId,
                AssetId = dto.AssetId,
                Quantity = (decimal)dto.Quantity,
                CreatedDate = DateTime.UtcNow,
                Transaction = new Transaction
                {
                    PublicId = Guid.NewGuid(),
                    TransactionPrice = dto.Price,
                    IsBuy = dto.IsBuy,
                    RealizedPnl = realizedPnl,
                    CreatedDate = DateTime.UtcNow
                }
            };

            _marketContext.Movements.Add(movement);

            await _portfolioContext.SaveChangesAsync();
            await _marketContext.SaveChangesAsync();

            return (true, "Transacción exitosa", item.Quantity);
        }

        public async Task<(bool Success, string Message)> DeletePortfolioItemAsync(int portfolioId)
        {
            var item = await _portfolioContext.PortfolioItems.FindAsync(portfolioId);

            if (item == null)
            {
                return (false, "El activo no existe.");
            }
            _portfolioContext.PortfolioItems.Remove(item);
            await _portfolioContext.SaveChangesAsync();

            return (true, "Activo eliminado permanentemente del portafolio.");
        }
        public async Task<PortfolioItem> CreatePortfolioItemAsync(PortfolioItem item)
        {
            if (item.PublicId == Guid.Empty) item.PublicId = Guid.NewGuid();

            item.IsActive = true;
            item.CreatedAt = DateTime.UtcNow;
            item.UpdatedAt = DateTime.UtcNow;

            _portfolioContext.PortfolioItems.Add(item);
            await _portfolioContext.SaveChangesAsync();

            return item;
        }
        public async Task<(bool Success, string Message, Guid? WalletId, decimal? Balance)> UpdateWalletAsync(WalletDto dto)
        {
            var wallet = await _portfolioContext.UserWallets
                                .FirstOrDefaultAsync(w => w.MembershipId == dto.MembershipId);

            if (wallet == null)
            {
                wallet = new UserWallet
                {
                    PublicId = Guid.NewGuid(),
                    MembershipId = dto.MembershipId,
                    CashBalance = (double)dto.CashBalance,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _portfolioContext.UserWallets.Add(wallet);
            }
            else
            {
                wallet.CashBalance = (double)dto.CashBalance;
                wallet.UpdatedAt = DateTime.UtcNow;

                _portfolioContext.UserWallets.Update(wallet);
            }

            try
            {
                await _portfolioContext.SaveChangesAsync();
                return (true, "Wallet actualizada correctamente", wallet.PublicId, (decimal)wallet.CashBalance);
            }
            catch (Exception ex)
            {
                return (false, $"Error actualizando la wallet: {ex.Message}", null, null);
            }
        }
    }
}