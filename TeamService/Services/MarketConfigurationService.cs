using Microsoft.EntityFrameworkCore;
using TeamService.Data;
using TeamService.Dtos;
using TeamService.Mapper;

namespace TeamService.Services
{
    public class MarketConfigurationService
    {
        private readonly TeamDbContext _context;

        public MarketConfigurationService(TeamDbContext context)
        {
            _context = context;
        }

        public async Task<MarketConfigurationDto?> CreateAsync(MarketConfigurationDto dto)
        {
            var team = await _context.Teams
                .Include(t => t.MarketConfiguration)
                .FirstOrDefaultAsync(t => t.TeamId == dto.TeamId);

            if (team == null || team.MarketConfiguration != null) return null;

            var config = dto.ToEntity();
            config.CreatedAt = DateTime.UtcNow;

            _context.MarketConfigurations.Add(config);
            await _context.SaveChangesAsync();

            return config.ToDto();
        }

        public async Task<MarketConfigurationDto?> UpdateAsync(MarketConfigurationDto dto)
        {
            var config = await _context.MarketConfigurations
                .FirstOrDefaultAsync(c => c.TeamId == dto.TeamId);

            if (config == null) return null; 

            config.UpdateEntity(dto);
            await _context.SaveChangesAsync();

            return config.ToDto();
        }

        public async Task<MarketConfigurationDto?> GetByTeamAsync(int teamId)
        {
            var config = await _context.MarketConfigurations
                .FirstOrDefaultAsync(c => c.TeamId == teamId);

            return config?.ToDto();
        }
    }
}
