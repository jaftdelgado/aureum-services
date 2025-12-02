using System.Text.Json.Serialization;

namespace PortfolioService.Dtos
{
   
    public class PortfolioDto
    {
        public Guid UserId { get; set; }
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
      
        [JsonPropertyName("publicId")]
        public Guid Id { get; set; }

       
        [JsonPropertyName("assetName")]
        public string Name { get; set; }

        [JsonPropertyName("assetSymbol")]
        public string Symbol { get; set; }
    }
}