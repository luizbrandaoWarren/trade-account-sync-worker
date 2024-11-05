using StackExchange.Redis.Extensions.Core.Abstractions;
using Warren.Trade.Risk.ClientV2.Services.Interfaces;
using Warren.Trade.Risk.Infra.Models;

namespace Warren.Trade.Risk.ClientV2.Services;
public class TradeCacheService : ITradeCacheService
{
    private const int TRADE_REDIS_DB = 5;
    private const string TRADE_CUSTOMER_KEY = "trade-customer:{0}";

    private readonly IRedisDatabase _cache;

    public TradeCacheService(IRedisCacheClient redisCacheClient)
    {
        _cache = redisCacheClient.GetDb(TRADE_REDIS_DB);
    }

    public Task<TradeCustomer?> GetTradeCustomerAsync(string customerApiId)
    {
        var key = string.Format(TRADE_CUSTOMER_KEY, customerApiId);
        return _cache.GetAsync<TradeCustomer?>(key);
    }

    public async Task<bool> DeleteTradeCustomerAsync(string customerApiId)
    {
        var key = string.Format(TRADE_CUSTOMER_KEY, customerApiId);
        return await _cache.RemoveAsync(key);
    }
}
