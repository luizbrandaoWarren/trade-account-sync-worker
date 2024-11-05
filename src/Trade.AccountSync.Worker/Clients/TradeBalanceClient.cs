using Microsoft.Extensions.Logging;
using Warren.Core.FeatureFlag.Client.Implementation;
using Warren.Trade.Risk.ClientV2.Clients.Interfaces;

namespace Warren.Trade.Risk.ClientV2.Clients
{
    public class TradeBalanceClient : HttpClientBase, ITradeBalanceClient
    {
        private readonly ILogger<TradeBalanceClient> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public TradeBalanceClient(
            ILogger<TradeBalanceClient> logger,
            IHttpClientFactory httpClientFactory)
            : base(logger)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        public async Task UpdateAccountingBalanceAsync(string customerApiId)
        {
            _logger.LogDebug(
                "UpdateAccountingBalanceAsync(customerApiId={customerApiId})",
                customerApiId);

            try
            {
                await PostAsync<object, object>(
                    "api/v2/balances/updatebalance",
                    new[] { customerApiId });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error while updating accounting balance for customer api id: {customerApiId}",
                    customerApiId);
            }
        }

        protected override HttpClient CreateHttpClient()
        {
            return _httpClientFactory.CreateClient(nameof(TradeBalanceClient));
        }
    }
}
