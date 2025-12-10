using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace PortfolioService.Dtos
{
    /// <summary>
    /// Resumen del estado actual de un activo dentro del portafolio del usuario.
    /// </summary>
    public class PortfolioDto
    {
        /// <summary>
        /// ID interno del portafolio.
        /// </summary>
        public int PortfolioId { get; set; }

        /// <summary>
        /// ID del usuario propietario.
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// ID del activo.
        /// </summary>
        public Guid AssetId { get; set; }

        /// <summary>
        /// Nombre del activo.
        /// </summary>
        public string AssetName { get; set; } = string.Empty;

        /// <summary>
        /// Símbolo o Ticker del activo.
        /// </summary>
        public string AssetSymbol { get; set; } = string.Empty;

        /// <summary>
        /// Cantidad actual poseída.
        /// </summary>
        public double Quantity { get; set; }

        /// <summary>
        /// Precio promedio de compra (Avg Price).
        /// </summary>
        public double AvgPrice { get; set; }

        /// <summary>
        /// Valor actual de mercado del activo (Unitario).
        /// </summary>
        public double CurrentValue { get; set; }

        /// <summary>
        /// Total invertido (Costo base).
        /// </summary>
        public double TotalInvestment { get; set; }

        /// <summary>
        /// Valor total actual (Cantidad * Precio Actual).
        /// </summary>
        public double CurrentTotalValue { get; set; }

        /// <summary>
        /// Ganancia o pérdida no realizada (PnL flotante).
        /// </summary>
        public double ProfitOrLoss { get; set; }

        /// <summary>
        /// Porcentaje de ganancia o pérdida.
        /// </summary>
        public double ProfitOrLossPercentage { get; set; }
    }

    /// <summary>
    /// Respuesta genérica para operaciones que solo requieren confirmación.
    /// </summary>
    public class GenericResponseDto
    {
        /// <summary>
        /// Mensaje del resultado de la operación.
        /// </summary>
        /// <example>El ítem fue eliminado correctamente.</example>
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// Objeto de Transferencia de Datos (DTO) que representa la información pública de un activo financiero.
    /// Se utiliza para enviar datos básicos del activo al cliente, ocultando detalles internos de la base de datos.
    /// </summary>
    public class AssetExternalDto
    {
        /// <summary>
        /// Identificador único universal (GUID) del activo para uso público.
        /// </summary>
        /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
        [JsonPropertyName("publicId")]
        public Guid Id { get; set; }

        /// <summary>
        /// Nombre completo o comercial del activo (ej. "Apple Inc.", "Bitcoin").
        /// </summary>
        [JsonPropertyName("assetName")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Símbolo o "ticker" bursátil que identifica al activo en el mercado (ej. "AAPL", "BTC").
        /// </summary>
        [JsonPropertyName("assetSymbol")]
        public string Symbol { get; set; } = string.Empty;
    }
    /// <summary>
    /// DTO para mapear la respuesta externa del servicio de Cursos/Equipos (TeamService).
    /// </summary>
    public class MembershipExternalDto
    {
        /// <summary>
        /// El ID público de la membresía (GUID).
        /// Mapea desde "public_id" o "publicid".
        /// </summary>
        [JsonPropertyName("public_id")]
        [JsonProperty("public_id")]
        public Guid MembershipId { get; set; }

        /// <summary>
        /// El ID del equipo al que pertenece.
        /// Mapea desde "team_id".
        /// </summary>
        [JsonPropertyName("team_id")]
        [JsonProperty("team_id")]
        public Guid TeamId { get; set; }

        /// <summary>
        /// El ID del usuario estudiante.
        /// Mapea desde "user_id".
        /// </summary>
        [JsonPropertyName("user_id")]
        [JsonProperty("user_id")]
        public Guid UserId { get; set; }
    }

    /// <summary>
    /// DTO ligero para devolver la cantidad de un activo en posesión.
    /// </summary>
    public class AssetQuantityDto
    {
        /// <summary>
        /// Identificador del activo consultado.
        /// </summary>
        public Guid AssetId { get; set; }

        /// <summary>
        /// Cantidad actual disponible en el portafolio.
        /// </summary>
        public double Quantity { get; set; }
    }
}