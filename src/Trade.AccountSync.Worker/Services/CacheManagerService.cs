using Warren.Trade.Risk.ClientV2.Clients.Interfaces;
using Warren.Trade.Risk.ClientV2.Services.Interfaces;
using Warren.Trade.Risk.Infra.Exceptions;

namespace Warren.Trade.Risk.ClientV2.Services;
public class CacheManagerService : ICacheManagerService
{
    private readonly ITradeOrdersClient _tradeOrdersClient;
    private readonly ITradeCacheService _tradeCacheService;

    public CacheManagerService(
        ITradeCacheService tradeCacheService,
        ITradeOrdersClient tradeOrdersClient)
    {
        _tradeOrdersClient = tradeOrdersClient;
        _tradeCacheService = tradeCacheService;
    }

    public async Task PurgeCachesAsync(string customerApiId)
    {
        var wasRemovedFromRedis = await _tradeCacheService.DeleteTradeCustomerAsync(customerApiId);

        if (!wasRemovedFromRedis)
        {
            throw new CacheInvalidationException("Cache on Redis has not been deleted");
        }

        await _tradeOrdersClient.DeleteCustomerCacheAsync(customerApiId);
    }
}