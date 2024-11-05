using Warren.Trade.Risk.Infra.Models;

namespace Warren.Trade.Risk.ClientV2.Services.Interfaces;
public interface ITradeCacheService
{
    Task<TradeCustomer?> GetTradeCustomerAsync(string customerApiId);
    Task<bool> DeleteTradeCustomerAsync(string customerApiId);
}
