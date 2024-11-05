namespace Warren.Trade.Risk.ClientV2.Clients.Interfaces;

public interface ITradeBalanceClient
{
    Task UpdateAccountingBalanceAsync(string customerApiId);
}