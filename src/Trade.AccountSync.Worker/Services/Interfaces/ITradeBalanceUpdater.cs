namespace Warren.Trade.Risk.ClientV2.Services.Interfaces;

public interface ITradeBalanceUpdater
{
    Task UpdateTradeBalanceAsync(string customerApiId, int? portfolioId);
}
