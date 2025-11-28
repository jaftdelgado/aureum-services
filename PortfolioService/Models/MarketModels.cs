using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PortfolioService.Models
{
    [Table("movements", Schema = "public")]
    public class Movement
    {
        [Key]
        [Column("movementid")]
        public int MovementId { get; set; }

        [Column("publicid")]
        public Guid PublicId { get; set; }

        [Column("userid")]
        public Guid UserId { get; set; }

        [Column("assetid")]
        public Guid AssetId { get; set; }

        [Column("quantity")]
        public decimal Quantity { get; set; }

        [Column("createddate")]
        public DateTime CreatedDate { get; set; }

        
        public Transaction? Transaction { get; set; }
    }

    [Table("transactions", Schema = "public")]
    public class Transaction
    {
        [Key]
        [Column("transactionid")]
        public int TransactionId { get; set; }

        [Column("publicid")]
        public Guid PublicId { get; set; }

        [Column("movementid")]
        public int MovementId { get; set; }

        [Column("transactionprice")]
        public decimal TransactionPrice { get; set; }

        [Column("isbuy")]
        public bool IsBuy { get; set; }

        [Column("createddate")]
        public DateTime CreatedDate { get; set; }

        [ForeignKey("MovementId")]
        public Movement? Movement { get; set; }
    }
}