using Microsoft.AspNetCore.Mvc;
using TeamService.Dtos;
using TeamService.Services;

namespace TeamService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MarketConfigurationController : ControllerBase
    {
        private readonly MarketConfigurationService _service;

        public MarketConfigurationController(MarketConfigurationService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] MarketConfigurationDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existingConfig = await _service.GetByTeamAsync(dto.TeamId);
            if (existingConfig != null)
                return Conflict($"La configuración para el equipo {dto.TeamId} ya existe.");

            var createdDto = await _service.CreateAsync(dto);
            if (createdDto == null)
                return NotFound($"No se encontró el equipo con ID {dto.TeamId}.");

            return CreatedAtAction(nameof(GetByTeam), new { teamId = dto.TeamId }, createdDto);
        }

        [HttpPut("{teamId}")]
        public async Task<IActionResult> Update(int teamId, [FromBody] MarketConfigurationDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (teamId != dto.TeamId)
                return BadRequest("El ID del equipo en la URL no coincide con el DTO.");

            var existingConfig = await _service.GetByTeamAsync(teamId);
            if (existingConfig == null)
                return NotFound($"No se encontró configuración para el equipo {teamId}.");

            var updatedDto = await _service.UpdateAsync(dto);
            return Ok(updatedDto);
        }

        [HttpGet("{teamId}")]
        public async Task<IActionResult> GetByTeam(int teamId)
        {
            var configDto = await _service.GetByTeamAsync(teamId);
            if (configDto == null)
                return NotFound($"No se encontró configuración para el equipo {teamId}");

            return Ok(configDto);
        }
    }
}
