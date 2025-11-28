namespace PortfolioService.Dtos
{
   
    public class PortfolioDto
    {
        public int PortfolioId { get; set; }
        public Guid AssetId { get; set; } 
        public double Quantity { get; set; }
        public double AvgPrice { get; set; }      
        public double CurrentValue { get; set; }  

    
        public string AssetName { get; set; }
        public string AssetSymbol { get; set; }

      
        public double TotalInvestment { get; set; } 
        public double CurrentTotalValue { get; set; } 
        public double ProfitOrLoss { get; set; }   
        public double ProfitOrLossPercentage { get; set; } 
    }

    
    public class AssetExternalDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Symbol { get; set; }
    }
}