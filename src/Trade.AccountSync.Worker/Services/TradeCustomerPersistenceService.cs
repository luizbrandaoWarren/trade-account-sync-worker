using Microsoft.Extensions.Logging;
using StackExchange.Redis.Extensions.Core.Abstractions;
using Warren.Trade.Risk.ClientV2.Services.Interfaces;
using Warren.Trade.Risk.Infra.Models;

namespace Warren.Trade.Risk.ClientV2.Services
{
    public class TradeCustomerPersistenceService : ITradeCustomerPersistenceService
    {
        private readonly ILogger<TradeCustomerPersistenceService> _logger;
        private readonly IRedisDatabase _tradeBalanceDb;

        private const int TRADE_REDIS_DB = 5;
        private const string TRADE_CUSTOMER_DB = "trade-customer:{0}";

        public TradeCustomerPersistenceService(
            ILogger<TradeCustomerPersistenceService> logger,
            IRedisCacheClient redisCacheClient)
        {
            _logger = logger;
            _tradeBalanceDb = redisCacheClient.GetDb(TRADE_REDIS_DB);
        }

        public async Task SaveTradeCustomer(string apiID, int externalId, int tradePortfolioId)
        {
            var key = string.Format(TRADE_CUSTOMER_DB, apiID);
            var value = new TradeCustomer
            {
                ExternalId = externalId,
                PortfolioId = tradePortfolioId
            };

            await _tradeBalanceDb.AddAsync(key, value);
        }
    }
}
