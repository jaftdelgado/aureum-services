using System.Text.Json.Serialization;
using Newtonsoft.Json;
namespace PortfolioService.Dtos
 
{
    public class PortfolioTransactionDto
    {
       [JsonPropertyName("userPublicId")]
        [JsonProperty("userPublicId")] 
        public Guid UserId { get; set; }

        [JsonPropertyName("teamPublicId")]
        [JsonProperty("teamPublicId")]
        public Guid TeamId { get; set; }

        [JsonPropertyName("assetPublicId")]
        [JsonProperty("assetPublicId")]
        public Guid AssetId { get; set; }
       
        public double Quantity { get; set; }
        public decimal Price { get; set; } 
        public bool IsBuy { get; set; } = true;
    }
}
