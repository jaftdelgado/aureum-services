namespace PortfolioService.Dtos
 using System.Text.Json.Serialization;
{
    public class PortfolioTransactionDto
    {
        [JsonPropertyName("userPublicId")]
        public Guid UserId { get; set; }

        [JsonPropertyName("teamPublicId")]
        public Guid TeamId { get; set; }

        [JsonPropertyName("assetPublicId")]
        public Guid AssetId { get; set; }
       
        public double Quantity { get; set; }
        public decimal Price { get; set; } 
        public bool IsBuy { get; set; } = true;
    }
}
