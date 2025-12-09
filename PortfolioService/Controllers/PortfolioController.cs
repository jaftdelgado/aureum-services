using Microsoft.AspNetCore.Mvc;
using PortfolioService.Dtos;
using PortfolioService.Models;
using PortfolioService.Services;

namespace PortfolioService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PortfolioController : ControllerBase
    {
        private readonly IPortfolioManagementService _portfolioService;
        public PortfolioController(IPortfolioManagementService portfolioService)
        {
            _portfolioService = portfolioService;
        }

        [HttpGet("course/{courseId}")]
        public async Task<ActionResult<IEnumerable<PortfolioDto>>> GetByCourse(Guid courseId)
        {
            var result = await _portfolioService.GetPortfolioByCourseAsync(courseId);
            return Ok(result);
        }

        [HttpGet("detail/{portfolioId}")]
        public async Task<ActionResult<PortfolioDto>> GetPortfolioDetail(int portfolioId)
        {
            var result = await _portfolioService.GetPortfolioDetailAsync(portfolioId);
            if (result == null) return NotFound("El activo no existe en el portafolio.");
            return Ok(result);
        }

        [HttpGet("history/course/{courseId}/student/{studentId}")]
        public async Task<ActionResult<IEnumerable<HistoryDto>>> GetHistory(Guid courseId, Guid studentId)
        {
            var result = await _portfolioService.GetHistoryAsync(courseId, studentId);
            return Ok(result);
        }

        [HttpPost("transaction")]
        public async Task<IActionResult> RegisterSmartTransaction([FromBody] PortfolioTransactionDto dto)
        {
            if (dto.UserId == Guid.Empty || dto.TeamId == Guid.Empty || dto.AssetId == Guid.Empty)
            {
                return BadRequest(new { message = "Los IDs no pueden estar vacíos." });
            }

            var result = await _portfolioService.ProcessTransactionAsync(dto);

            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }

            return Ok(new { message = result.Message, currentQuantity = result.CurrentQuantity });
        }

        [HttpDelete("{portfolioId}")]
        public async Task<IActionResult> DeletePortfolioItem(int portfolioId)
        {
            var result = await _portfolioService.DeletePortfolioItemAsync(portfolioId);

            if (!result.Success)
            {
                return NotFound(new { message = result.Message });
            }

            return Ok(new { message = result.Message });
        }

        [HttpPost]
        public async Task<ActionResult<PortfolioItem>> PostPortfolioItem(PortfolioItem item)
        {
            if (item.UserId == Guid.Empty || item.AssetId == Guid.Empty || item.TeamId == Guid.Empty)
            {
                return BadRequest("UserId, AssetId y TeamId son obligatorios.");
            }

            var createdItem = await _portfolioService.CreatePortfolioItemAsync(item);

            return CreatedAtAction(nameof(GetByCourse), new { courseId = createdItem.TeamId }, createdItem);
        }

        [HttpPut("wallet")]
        public async Task<IActionResult> UpdateWallet([FromBody] WalletDto dto)
        {
            if (dto.MembershipId == Guid.Empty)
            {
                return BadRequest(new { message = "El MembershipId es obligatorio." });
            }
            var result = await _portfolioService.UpdateWalletAsync(dto);

            if (!result.Success)
            {
                return StatusCode(500, new { message = result.Message });
            }
            return Ok(new { message = result.Message, walletId = result.WalletId, balance = result.Balance });
        }
        [HttpGet("history/team/{teamId}")]
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
    }
}