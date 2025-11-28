namespace PortfolioService.Dtos
{
    public class HistoryDto
    {
        public Guid MovementId { get; set; }
        public Guid AssetId { get; set; }

       
        public string AssetName { get; set; }
        public string AssetSymbol { get; set; }

        public decimal Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal TotalAmount { get; set; } 
        public string Type { get; set; } 
        public DateTime Date { get; set; }
    }
}