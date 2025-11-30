namespace PortfolioService.Dtos
{
    public class PortfolioUpdateDto
    {
        public double Quantity { get; set; }
        public double AvgPrice { get; set; }
        public string? Notes { get; set; }
    }
}