namespace PortfolioService.Dtos
{
    public class PortfolioTransactionDto
    {
        public Guid UserId { get; set; }
        public Guid TeamId { get; set; }
        public Guid AssetId { get; set; }

       
        public double Quantity { get; set; }
        public decimal Price { get; set; } 
        public bool IsBuy { get; set; }
    }
}