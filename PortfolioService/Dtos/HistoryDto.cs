namespace PortfolioService.Dtos
{
    public class HistoryDto
    {
        public Guid MovementId { get; set; }
        public Guid AssetId { get; set; }
        public string AssetName { get; set; } = string.Empty;
        public string AssetSymbol { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal TotalAmount { get; set; }
        public string Type { get; set; } = string.Empty;
        public decimal RealizedPnl { get; set; }
        public DateTime Date { get; set; }
    }
    public class PaginatedResponseDto<T>
    {
        public IEnumerable<T> Items { get; set; }
        public int TotalItems { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);
    }
}