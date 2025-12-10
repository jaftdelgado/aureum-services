using Microsoft.AspNetCore.Mvc;
using PortfolioService.Dtos;
using PortfolioService.Models;
using PortfolioService.Services;

namespace PortfolioService.Controllers
{
    /// <summary>
    /// Controlador encargado de la gestión de portafolios de inversión, historiales de transacciones y balances de billeteras.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class PortfolioController : ControllerBase
    {
        private readonly IPortfolioManagementService _portfolioService;

        public PortfolioController(IPortfolioManagementService portfolioService)
        {
            _portfolioService = portfolioService;
        }

        /// <summary>
        /// Obtiene el portafolio completo asociado a un curso o equipo específico.
        /// </summary>
        /// <param name="courseId">El identificador único (GUID) del curso o equipo.</param>
        /// <returns>Una lista de objetos <see cref="PortfolioDto"/> con los activos actuales.</returns>
        /// <response code="200">Retorna la lista de activos del portafolio.</response>
        [HttpGet("course/{courseId}")]
        [ProducesResponseType(typeof(IEnumerable<PortfolioDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<PortfolioDto>>> GetByCourse(Guid courseId)
        {
            var result = await _portfolioService.GetPortfolioByCourseAsync(courseId);
            return Ok(result);
        }

        /// <summary>
        /// Obtiene el detalle de un ítem específico dentro del portafolio.
        /// </summary>
        /// <param name="portfolioId">El ID numérico interno del registro del portafolio.</param>
        /// <returns>Los detalles del activo en el portafolio.</returns>
        /// <response code="200">Retorna el detalle del ítem solicitado.</response>
        /// <response code="404">Si el ítem no existe en la base de datos.</response>
        [HttpGet("detail/{portfolioId}")]
        [ProducesResponseType(typeof(PortfolioDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PortfolioDto>> GetPortfolioDetail(int portfolioId)
        {
            var result = await _portfolioService.GetPortfolioDetailAsync(portfolioId);
            if (result == null) return NotFound("El activo no existe en el portafolio.");
            return Ok(result);
        }

        /// <summary>
        /// Consulta el historial de transacciones de un estudiante específico dentro de un curso.
        /// </summary>
        /// <param name="courseId">El ID del curso.</param>
        /// <param name="studentId">El ID del estudiante.</param>
        /// <returns>Una lista cronológica de transacciones.</returns>
        /// <response code="200">Retorna el historial encontrado.</response>
        [HttpGet("history/course/{courseId}/student/{studentId}")]
        [ProducesResponseType(typeof(IEnumerable<HistoryDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<HistoryDto>>> GetHistory(Guid courseId, Guid studentId)
        {
            var result = await _portfolioService.GetHistoryAsync(courseId, studentId);
            return Ok(result);
        }

        /// <summary>
        /// Procesa una transacción inteligente (Compra/Venta) en el portafolio.
        /// </summary>
        /// <remarks>
        /// Valida fondos, actualiza la cantidad de activos y registra el historial.
        /// </remarks>
        /// <param name="dto">Objeto con la información de la transacción (Usuario, Activo, Tipo, Cantidad).</param>
        /// <returns>Un mensaje de éxito y la cantidad actual del activo.</returns>
        /// <response code="200">Transacción exitosa.</response>
        /// <response code="400">Si faltan IDs o la lógica de negocio falla (ej. fondos insuficientes).</response>
        [HttpPost("transaction")]
        [ProducesResponseType(typeof(TransactionResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GenericResponseDto), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RegisterSmartTransaction([FromBody] PortfolioTransactionDto dto)
        {
            if (dto.UserId == Guid.Empty || dto.TeamId == Guid.Empty || dto.AssetId == Guid.Empty)
            {
                return BadRequest(new GenericResponseDto { Message = "Los IDs no pueden estar vacíos." });
            }

            var result = await _portfolioService.ProcessTransactionAsync(dto);

            if (!result.Success)
            {
                return BadRequest(new GenericResponseDto { Message = result.Message });
            }

            return Ok(new TransactionResponseDto
            {
                Message = result.Message,
                CurrentQuantity = (decimal)result.CurrentQuantity
            });
        }

        /// <summary>
        /// Elimina un ítem del portafolio por su ID.
        /// </summary>
        /// <param name="portfolioId">El ID numérico del ítem a eliminar.</param>
        /// <returns>Mensaje de confirmación.</returns>
        /// <response code="200">Eliminación exitosa.</response>
        /// <response code="404">No se encontró el ítem o no se pudo eliminar.</response>
        [HttpDelete("{portfolioId}")]
        [ProducesResponseType(typeof(GenericResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GenericResponseDto), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeletePortfolioItem(int portfolioId)
        {
            var result = await _portfolioService.DeletePortfolioItemAsync(portfolioId);

            if (!result.Success)
            {
                return NotFound(new GenericResponseDto { Message = result.Message });
            }

            return Ok(new GenericResponseDto { Message = result.Message });
        }

        /// <summary>
        /// Crea manualmente un ítem en el portafolio (generalmente para inicialización).
        /// </summary>
        /// <param name="item">La entidad PortfolioItem a crear.</param>
        /// <returns>El ítem creado con su nueva ubicación.</returns>
        /// <response code="201">Recurso creado exitosamente.</response>
        /// <response code="400">Si faltan datos obligatorios (UserId, AssetId, TeamId).</response>
        [HttpPost]
        [ProducesResponseType(typeof(PortfolioItem), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<PortfolioItem>> PostPortfolioItem(PortfolioItem item)
        {
            if (item.UserId == Guid.Empty || item.AssetId == Guid.Empty || item.TeamId == Guid.Empty)
            {
                return BadRequest("UserId, AssetId y TeamId son obligatorios.");
            }

            var createdItem = await _portfolioService.CreatePortfolioItemAsync(item);

            return CreatedAtAction(nameof(GetByCourse), new { courseId = createdItem.TeamId }, createdItem);
        }

        /// <summary>
        /// Actualiza el saldo de la billetera (Wallet) asociada a una membresía.
        /// </summary>
        /// <param name="dto">Datos para la actualización de la billetera.</param>
        /// <returns>El nuevo balance y el ID de la billetera.</returns>
        /// <response code="200">Billetera actualizada correctamente.</response>
        /// <response code="400">Si falta el MembershipId.</response>
        /// <response code="500">Error al procesar la actualización.</response>
        [HttpPut("wallet")]
        [ProducesResponseType(typeof(WalletUpdateResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GenericResponseDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GenericResponseDto), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateWallet([FromBody] WalletDto dto)
        {
            if (dto.MembershipId == Guid.Empty)
            {
                return BadRequest(new GenericResponseDto { Message = "El MembershipId es obligatorio." });
            }

            var result = await _portfolioService.UpdateWalletAsync(dto);

            if (!result.Success)
            {
                return StatusCode(500, new GenericResponseDto { Message = result.Message });
            }

            return Ok(new WalletUpdateResponseDto
            {
                Message = result.Message,
                WalletId = result.WalletId ?? Guid.Empty,
                Balance = result.Balance ?? 0m
            });
        }

        /// <summary>
        /// Obtiene el historial de transacciones de un equipo de forma paginada.
        /// </summary>
        /// <param name="teamId">El ID del equipo a consultar.</param>
        /// <param name="page">Número de página (por defecto 1).</param>
        /// <param name="pageSize">Cantidad de registros por página (por defecto 10, máx 100).</param>
        /// <returns>Una respuesta paginada con la lista de historiales.</returns>
        /// <response code="200">Retorna la página solicitada del historial.</response>
        [HttpGet("history/team/{teamId}")]
        [ProducesResponseType(typeof(PaginatedResponseDto<HistoryDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PaginatedResponseDto<HistoryDto>>> GetTeamHistory(
                    Guid teamId,
                    [FromQuery] int page = 1,
                    [FromQuery] int pageSize = 10)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;
            var result = await _portfolioService.GetTeamHistoryAsync(teamId, page, pageSize);
            return Ok(result);
        }
        /// <summary>
        /// Obtiene las billeteras de los miembros de un equipo de forma paginada.
        /// </summary>
        [HttpGet("wallets/team/{teamId}")]
        [ProducesResponseType(typeof(PaginatedResponseDto<WalletDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PaginatedResponseDto<WalletDto>>> GetTeamWallets(
            Guid teamId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var result = await _portfolioService.GetTeamWalletsAsync(teamId, page, pageSize);
            return Ok(result);
        }

        /// <summary>
        /// Obtiene los ítems del portafolio de un equipo de forma paginada.
        /// </summary>
        [HttpGet("items/team/{teamId}")]
        [ProducesResponseType(typeof(PaginatedResponseDto<PortfolioDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PaginatedResponseDto<PortfolioDto>>> GetTeamPortfolioItems(
            Guid teamId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var result = await _portfolioService.GetTeamPortfoliosPaginatedAsync(teamId, page, pageSize);
            return Ok(result);
        }

        /// <summary>
        /// Obtiene la lista de todos los activos que posee un usuario en un equipo con sus cantidades.
        /// </summary>
        [HttpGet("assets/team/{teamId}/user/{userId}")]
        [ProducesResponseType(typeof(IEnumerable<AssetQuantityDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<AssetQuantityDto>>> GetUserAssets(Guid teamId, Guid userId)
        {
            var result = await _portfolioService.GetUserAssetsAsync(teamId, userId);
            return Ok(result);
        }
    }
}