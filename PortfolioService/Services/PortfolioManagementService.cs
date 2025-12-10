using Microsoft.EntityFrameworkCore;
using PortfolioService.Data;
using PortfolioService.Dtos;
using PortfolioService.Models;
using PortfolioService.Services.External;


namespace PortfolioService.Services
{/// <summary>
 /// Define la lógica de negocio principal para la gestión de portafolios, transacciones y balances.
 /// </summary>
    public interface IPortfolioManagementService
    {
        /// <summary>
        /// Obtiene el listado de activos que conforman el portafolio de un curso o equipo.
        /// Incluye cálculos de valor actual y PnL (Ganancia/Pérdida) en tiempo real.
        /// </summary>
        /// <param name="courseId">Identificador único del curso o equipo (TeamId).</param>
        /// <returns>Una colección de DTOs con el estado actual de cada activo en el portafolio.</returns>
        Task<IEnumerable<PortfolioDto>> GetPortfolioByCourseAsync(Guid courseId);

        /// <summary>
        /// Recupera el historial de movimientos (compras/ventas) de un estudiante específico en un curso.
        /// </summary>
        /// <param name="courseId">Identificador del curso.</param>
        /// <param name="studentId">Identificador del estudiante.</param>
        /// <returns>Lista cronológica de las transacciones realizadas.</returns>
        Task<IEnumerable<HistoryDto>> GetHistoryAsync(Guid courseId, Guid studentId);

        /// <summary>
        /// Obtiene los detalles profundos de un ítem específico del portafolio por su ID interno.
        /// </summary>
        /// <param name="portfolioId">Clave primaria (int) del registro en la tabla PortfolioItems.</param>
        /// <returns>
        /// Un objeto <see cref="PortfolioDto"/> si se encuentra; de lo contrario, <c>null</c>.
        /// </returns>
        Task<PortfolioDto?> GetPortfolioDetailAsync(int portfolioId);

        /// <summary>
        /// Procesa una transacción de compra o venta, aplicando validaciones de fondos y lógica de promedio de costos.
        /// </summary>
        /// <param name="dto">Datos de la transacción (Usuario, Activo, Cantidad, Precio, Tipo).</param>
        /// <returns>
        /// Una tupla que contiene:
        /// <br/>- <c>Success</c>: Indica si la operación fue exitosa.
        /// <br/>- <c>Message</c>: Explicación del resultado o error.
        /// <br/>- <c>CurrentQuantity</c>: La cantidad final del activo tras la operación.
        /// </returns>
        Task<(bool Success, string Message, double CurrentQuantity)> ProcessTransactionAsync(PortfolioTransactionDto dto);

        /// <summary>
        /// Elimina físicamente un registro del portafolio.
        /// </summary>
        /// <param name="portfolioId">ID del ítem a eliminar.</param>
        /// <returns>Tupla indicando éxito y mensaje de confirmación.</returns>
        Task<(bool Success, string Message)> DeletePortfolioItemAsync(int portfolioId);

        /// <summary>
        /// Crea un nuevo ítem en el portafolio directamente (usado para inicialización o administración).
        /// </summary>
        /// <param name="item">Entidad <see cref="PortfolioItem"/> a insertar.</param>
        /// <returns>El ítem creado con sus datos actualizados (Ids, fechas).</returns>
        Task<PortfolioItem> CreatePortfolioItemAsync(PortfolioItem item);

        /// <summary>
        /// Actualiza o crea la billetera (Wallet) de un usuario asociada a una membresía.
        /// </summary>
        /// <param name="dto">Datos del nuevo balance.</param>
        /// <returns>
        /// Una tupla con el estado de éxito, mensaje, y los datos actualizados de la wallet (ID y Balance).
        /// </returns>
        Task<(bool Success, string Message, Guid? WalletId, decimal? Balance)> UpdateWalletAsync(WalletDto dto);

        /// <summary>
        /// Obtiene el historial de transacciones de todo un equipo con paginación.
        /// </summary>
        /// <param name="teamId">Identificador del equipo.</param>
        /// <param name="page">Número de página actual (inicia en 1).</param>
        /// <param name="pageSize">Cantidad de registros por página.</param>
        /// <returns>Objeto paginado que incluye la lista de historiales y el conteo total.</returns>
        Task<PaginatedResponseDto<HistoryDto>> GetTeamHistoryAsync(Guid teamId, int page, int pageSize);
        /// <summary>
        /// Obtiene las billeteras de todos los alumnos de un equipo, paginadas.
        /// </summary>
        Task<PaginatedResponseDto<WalletDto>> GetTeamWalletsAsync(Guid teamId, int page, int pageSize);

        /// <summary>
        /// Obtiene todos los activos (portfolios) de un equipo, paginados.
        /// </summary>
        Task<PaginatedResponseDto<PortfolioDto>> GetTeamPortfoliosPaginatedAsync(Guid teamId, int page, int pageSize);

        /// <summary>
        /// Obtiene la cantidad exacta que un usuario tiene de un activo específico.
        /// </summary>
        Task<AssetQuantityDto> GetAssetQuantityAsync(Guid teamId, Guid userId, Guid assetId);
    }


    public class PortfolioManagementService : IPortfolioManagementService
    {
        private readonly PortfolioContext _portfolioContext;
        private readonly MarketContext _marketContext;
        private readonly IAssetGateway _assetGateway;
        private readonly ICourseGateway _courseGateway;

        public PortfolioManagementService(
                PortfolioContext portfolioContext,
                MarketContext marketContext,
                IAssetGateway assetGateway,
                ICourseGateway courseGateway)
        {
            _portfolioContext = portfolioContext;
            _marketContext = marketContext;
            _assetGateway = assetGateway;
            _courseGateway = courseGateway;
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
        public async Task<PaginatedResponseDto<HistoryDto>> GetTeamHistoryAsync(Guid teamId, int page, int pageSize)
        {
            var query = _marketContext.Movements
                                    .Include(m => m.Transaction)
                                    .Where(m => m.TeamId == teamId);
            var totalItems = await query.CountAsync();
            var movements = await query
                                    .OrderByDescending(m => m.CreatedDate)
                                    .Skip((page - 1) * pageSize)
                                    .Take(pageSize)
                                    .ToListAsync();

            if (!movements.Any())
            {
                return new PaginatedResponseDto<HistoryDto>
                {
                    Items = new List<HistoryDto>(),
                    TotalItems = 0,
                    Page = page,
                    PageSize = pageSize
                };
            }
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
                    AssetName = assetInfo?.Name ?? "Unknown",
                    AssetSymbol = assetInfo?.Symbol ?? "???",
                    Quantity = mov.Quantity,
                    Price = price,
                    TotalAmount = mov.Quantity * price,
                    Type = isBuy ? "Compra" : "Venta",
                    RealizedPnl = pnl,
                    Date = mov.CreatedDate
                };
            });
            var resultItems = await Task.WhenAll(tasks);

            return new PaginatedResponseDto<HistoryDto>
            {
                Items = resultItems,
                TotalItems = totalItems,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<PaginatedResponseDto<WalletDto>> GetTeamWalletsAsync(Guid teamId, int page, int pageSize)
        {
            var memberships = await _courseGateway.GetMembershipsByTeamAsync(teamId);

            var membershipIds = memberships.Select(m => m.MembershipId).ToList();

            if (!membershipIds.Any())
            {
                return new PaginatedResponseDto<WalletDto>
                {
                    Items = new List<WalletDto>(),
                    TotalItems = 0,
                    Page = page,
                    PageSize = pageSize
                };
            }

            var query = _portfolioContext.UserWallets
                            .Where(w => membershipIds.Contains(w.MembershipId));

            var totalItems = await query.CountAsync();

            var wallets = await query
                            .OrderByDescending(w => w.CashBalance)
                            .Skip((page - 1) * pageSize)
                            .Take(pageSize)
                            .ToListAsync();

            var walletDtos = wallets.Select(w => new WalletDto
            {
                MembershipId = w.MembershipId,
                CashBalance = (decimal)w.CashBalance
            });

            return new PaginatedResponseDto<WalletDto>
            {
                Items = walletDtos,
                TotalItems = totalItems,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<PaginatedResponseDto<PortfolioDto>> GetTeamPortfoliosPaginatedAsync(Guid teamId, int page, int pageSize)
        {
            var query = _portfolioContext.PortfolioItems
                            .Where(p => p.TeamId == teamId && p.IsActive);

            var totalItems = await query.CountAsync();

            var items = await query
                            .OrderBy(p => p.UserId)
                            .Skip((page - 1) * pageSize)
                            .Take(pageSize)
                            .ToListAsync();

            if (!items.Any())
            {
                return new PaginatedResponseDto<PortfolioDto>
                {
                    Items = new List<PortfolioDto>(),
                    TotalItems = totalItems,
                    Page = page,
                    PageSize = pageSize
                };
            }

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

            var resultItems = await Task.WhenAll(tasks);

            return new PaginatedResponseDto<PortfolioDto>
            {
                Items = resultItems,
                TotalItems = totalItems,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<AssetQuantityDto> GetAssetQuantityAsync(Guid teamId, Guid userId, Guid assetId)
        {
            var item = await _portfolioContext.PortfolioItems
                            .FirstOrDefaultAsync(p => p.TeamId == teamId
                                                   && p.UserId == userId
                                                   && p.AssetId == assetId
                                                   && p.IsActive);

            return new AssetQuantityDto
            {
                AssetId = assetId,
                Quantity = item?.Quantity ?? 0
            };
        }
    }
}