using TeamService.Models.Enums;

namespace TeamService.Dtos;

public class MarketConfigurationDto
{
    public int? ConfigId { get; set; }
    public int TeamId { get; set; }
    public double InitialCash { get; set; }
    public CurrencyEnum Currency { get; set; }
    public VolatilityEnum MarketVolatility { get; set; }
    public VolatilityEnum MarketLiquidity { get; set; }
    public ThickSpeedEnum ThickSpeed { get; set; }
    public TransactionFeeEnum TransactionFee { get; set; }
    public TransactionFeeEnum EventFrequency { get; set; }
    public TransactionFeeEnum DividendImpact { get; set; }
    public TransactionFeeEnum CrashImpact { get; set; }
    public bool AllowShortSelling { get; set; }
}
