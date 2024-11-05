namespace Warren.Trade.Risk.ClientV2.Services.Interfaces
{
    public interface ITradeCustomerPersistenceService
    {
        public Task SaveTradeCustomer(string apiID, int externalId, int tradePortfolioId);
    }
}
