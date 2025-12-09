namespace PortfolioService.Dtos
{
    /// <summary>
    /// Representa un registro histórico de una transacción o movimiento en el portafolio.
    /// </summary>
    public class HistoryDto
    {
        /// <summary>
        /// Identificador único del movimiento.
        /// </summary>
        public Guid MovementId { get; set; }

        /// <summary>
        /// Identificador del activo involucrado.
        /// </summary>
        public Guid AssetId { get; set; }

        /// <summary>
        /// Nombre legible del activo (ej. "Apple Inc.").
        /// </summary>
        public string AssetName { get; set; } = string.Empty;

        /// <summary>
        /// Símbolo del activo (ej. "AAPL").
        /// </summary>
        public string AssetSymbol { get; set; } = string.Empty;

        /// <summary>
        /// Cantidad transaccionada.
        /// </summary>
        public decimal Quantity { get; set; }

        /// <summary>
        /// Precio unitario al momento de la operación.
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// Monto total de la operación (Cantidad * Precio).
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Tipo de operación (ej. "Compra", "Venta").
        /// </summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// Ganancia o pérdida realizada (solo relevante en ventas).
        /// </summary>
        public decimal RealizedPnl { get; set; }

        /// <summary>
        /// Fecha y hora en que ocurrió el movimiento (UTC).
        /// </summary>
        public DateTime Date { get; set; }
    }
    /// <summary>
    /// Respuesta genérica para envolver listas paginadas de cualquier tipo de dato.
    /// </summary>
    /// <typeparam name="T">El tipo de objeto que contiene la lista (ej. HistoryDto).</typeparam>
    public class PaginatedResponseDto<T>
    {
        /// <summary>
        /// Colección de elementos correspondientes a la página actual.
        /// </summary>
        public IEnumerable<T> Items { get; set; } = new List<T>();

        /// <summary>
        /// Cantidad total de elementos disponibles en la base de datos (sin paginar).
        /// </summary>
        public int TotalItems { get; set; }

        /// <summary>
        /// Número de la página actual (índice base 1).
        /// </summary>
        public int Page { get; set; }

        /// <summary>
        /// Cantidad de elementos solicitados por página.
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Total de páginas disponibles calculado automáticamente.
        /// </summary>
        public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalItems / PageSize) : 0;
    }
}