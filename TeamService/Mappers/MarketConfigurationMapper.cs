using TeamService.Dtos;
using TeamService.Models;

namespace TeamService.Mapper
{
    public static class MarketConfigurationMapper
    {
        public static MarketConfigurationDto ToDto(this MarketConfiguration config)
        {
            if (config == null) return null!;

            return new MarketConfigurationDto
            {
                ConfigId = config.ConfigId,
                TeamId = config.TeamId,
                InitialCash = config.InitialCash,
                Currency = config.Currency,
                MarketVolatility = config.MarketVolatility,
                MarketLiquidity = config.MarketLiquidity,
                ThickSpeed = config.ThickSpeed,
                TransactionFee = config.TransactionFee,
                EventFrequency = config.EventFrequency,
                DividendImpact = config.DividendImpact,
                CrashImpact = config.CrashImpact,
                AllowShortSelling = config.AllowShortSelling ?? false
            };
        }

        public static MarketConfiguration ToEntity(this MarketConfigurationDto dto)
        {
            if (dto == null) return null!;

            return new MarketConfiguration
            {
                ConfigId = dto.ConfigId ?? 0,
                TeamId = dto.TeamId,
                InitialCash = dto.InitialCash,
                Currency = dto.Currency,
                MarketVolatility = dto.MarketVolatility,
                MarketLiquidity = dto.MarketLiquidity,
                ThickSpeed = dto.ThickSpeed,
                TransactionFee = dto.TransactionFee,
                EventFrequency = dto.EventFrequency,
                DividendImpact = dto.DividendImpact,
                CrashImpact = dto.CrashImpact,
                AllowShortSelling = dto.AllowShortSelling
            };
        }

        public static void UpdateEntity(this MarketConfiguration config, MarketConfigurationDto dto)
        {
            config.InitialCash = dto.InitialCash;
            config.Currency = dto.Currency;
            config.MarketVolatility = dto.MarketVolatility;
            config.MarketLiquidity = dto.MarketLiquidity;
            config.ThickSpeed = dto.ThickSpeed;
            config.TransactionFee = dto.TransactionFee;
            config.EventFrequency = dto.EventFrequency;
            config.DividendImpact = dto.DividendImpact;
            config.CrashImpact = dto.CrashImpact;
            config.AllowShortSelling = dto.AllowShortSelling;
            config.UpdatedAt = DateTime.UtcNow;
        }
    }
}
