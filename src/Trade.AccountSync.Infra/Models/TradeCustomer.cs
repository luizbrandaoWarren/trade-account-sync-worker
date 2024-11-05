namespace Warren.Trade.Risk.Infra.Models
{
    public record TradeCustomer
    {
        public int ExternalId { get; init; }
        public int PortfolioId { get; init; }
    }
}
