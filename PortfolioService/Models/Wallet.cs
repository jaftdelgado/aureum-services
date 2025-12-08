using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PortfolioService.Models
{
    [Table("userwallet", Schema = "public")]
    public class UserWallet
    {
        [Key]
        [Column("walletid")]
        public int WalletId { get; set; }

        [Column("publicid")]
        public Guid PublicId { get; set; }

        [Column("membershipid")]
        public Guid MembershipId { get; set; }

        [Column("cashbalance")]
        public double CashBalance { get; set; }

        [Column("createdat")]
        public DateTime CreatedAt { get; set; }

        [Column("updatedat")]
        public DateTime UpdatedAt { get; set; }
    }
}