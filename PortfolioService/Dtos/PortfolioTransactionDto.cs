using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PortfolioService.Dtos
{
    /// <summary>
    /// Objeto de transferencia para procesar transacciones de compra o venta de activos.
    /// </summary>
    public class PortfolioTransactionDto
    {
        /// <summary>
        /// Identificador único del usuario que realiza la transacción.
        /// </summary>
        [Required(ErrorMessage = "El userId es obligatorio.")]
        [JsonPropertyName("userId")]       
        [JsonProperty("userId")]          
        public Guid UserId { get; set; }

        /// <summary>
        /// Identificador del equipo o curso donde se realiza la operación.
        /// </summary>
        [Required(ErrorMessage = "El teamId es obligatorio.")]
        [JsonPropertyName("teamId")]      
        [JsonProperty("teamId")]           
        public Guid TeamId { get; set; }

        /// <summary>
        /// Identificador del activo financiero a transaccionar.
        /// </summary>
        [Required(ErrorMessage = "El assetId es obligatorio.")]
        [JsonPropertyName("assetId")]      
        [JsonProperty("assetId")]         
        public Guid AssetId { get; set; }

        /// <summary>
        /// Cantidad de unidades del activo. Debe ser mayor a 0.
        /// </summary>
        [Range(0.000001, double.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0.")]
        public double Quantity { get; set; }

        /// <summary>
        /// Precio unitario del activo al momento de la transacción.
        /// </summary>
        [Range(0, double.MaxValue, ErrorMessage = "El precio no puede ser negativo.")]
        public decimal Price { get; set; }

        /// <summary>
        /// Indica el tipo de operación: True para Compra, False para Venta.
        /// </summary>
        public bool IsBuy { get; set; } = true;
    }
    /// <summary>
    /// Respuesta devuelta tras procesar una transacción de compra o venta.
    /// </summary>
    public class TransactionResponseDto
    {
        /// <summary>
        /// Mensaje descriptivo del resultado de la operación.
        /// </summary>
        /// <example>Transacción realizada con éxito.</example>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// La nueva cantidad de activos que posee el usuario después de la transacción.
        /// </summary>
        /// <example>150.5</example>
        public decimal CurrentQuantity { get; set; }
    }
}