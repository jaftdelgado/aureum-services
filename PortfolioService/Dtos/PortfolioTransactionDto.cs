namespace PortfolioService.Dtos
{
    public class PortfolioTransactionDto
    {
        public double Quantity { get; set; }  
        public decimal Price { get; set; }     
        public bool IsBuy { get; set; }       
    }
}