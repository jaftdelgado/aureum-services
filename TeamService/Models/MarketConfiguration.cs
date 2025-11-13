using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TeamService.Models.Enums;

namespace TeamService.Models;

[Table("marketconfiguration", Schema = "public")]
public class MarketConfiguration
{
    [Key]
    [Column("configid")]
    public int ConfigId { get; set; }

    [Column("teamid")]
    [Required]
    public int TeamId { get; set; }

    [Column("initialcash")]
    [Required]
    public double InitialCash { get; set; }

    [Column("currency")]
    [Required]
    public CurrencyEnum Currency { get; set; }

    [Column("marketvolatility")]
    [Required]
    public VolatilityEnum MarketVolatility { get; set; }

    [Column("marketliquidity")]
    [Required]
    public VolatilityEnum MarketLiquidity { get; set; }

    [Column("thickspeed")]
    [Required]
    public ThickSpeedEnum ThickSpeed { get; set; }

    [Column("transactionfee")]
    [Required]
    public TransactionFeeEnum TransactionFee { get; set; }

    [Column("eventfrequency")]
    [Required]
    public TransactionFeeEnum EventFrequency { get; set; }

    [Column("dividendimpact")]
    [Required]
    public TransactionFeeEnum DividendImpact { get; set; }

    [Column("crashimpact")]
    [Required]
    public TransactionFeeEnum CrashImpact { get; set; }

    [Column("allowshortselling")]
    public bool? AllowShortSelling { get; set; } = false;

    [Column("createdat")]
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updatedat")]
    public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(TeamId))]
    public Team? Team { get; set; }
}
