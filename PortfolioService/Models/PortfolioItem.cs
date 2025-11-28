using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PortfolioService.Models
{
    
    [Table("portfolio", Schema = "public")]
    public class PortfolioItem
    {
        [Key]
        [Column("portfolioid")]
        public int PortfolioId { get; set; }

        [Column("publicid")]
        public Guid PublicId { get; set; } 

        [Column("userid")]
        public Guid UserId { get; set; }

        [Column("assetid")]
        public Guid AssetId { get; set; }

        [Column("teamid")]
        public Guid TeamId { get; set; }

        [Column("quantity")]
        public double Quantity { get; set; }

        [Column("avgprice")]
        public double AvgPrice { get; set; }

        [Column("currentvalue")]
        public double CurrentValue { get; set; }

        [Column("notes")]
        public string? Notes { get; set; } 

        [Column("isactive")]
        public bool IsActive { get; set; }

        [Column("createdat")]
        public DateTime CreatedAt { get; set; }

        [Column("updatedat")]
        public DateTime UpdatedAt { get; set; }
    }
}