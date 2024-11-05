namespace Warren.Trade.Risk.ClientV2.Clients.Interfaces;
public interface ITradeOrdersClient
{
    Task DeleteCustomerCacheAsync(string customerApiId);
}
