using Microsoft.Extensions.Logging;
using Warren.Core.FeatureFlag.Client.Implementation;
using Warren.Trade.Risk.ClientV2.Clients.Interfaces;
using Warren.Trade.Risk.ClientV2.Services.Interfaces;

namespace Warren.Trade.Risk.ClientV2.Services
{
    public class TradeBalanceUpdater : ITradeBalanceUpdater
    {
        private readonly ILogger<TradeBalanceUpdater> _logger;
        private readonly IFeatureFlagClient _featureFlagClient;
        private readonly ITradeBalanceClient _tradeBalanceClient;
        private readonly IMessageProducerService _messageProducerService;

        private const string BreakingChangeEnableUnifiedAccount = "br.com.warren.conta-corrente.breaking-change-enable-unified-account";

        public TradeBalanceUpdater(
            ILogger<TradeBalanceUpdater> logger,
            IFeatureFlagClient featureFlagClient,
            ITradeBalanceClient tradeBalanceClient,
            IMessageProducerService messageProducerService)
        {
            _logger = logger;
            _featureFlagClient = featureFlagClient;
            _tradeBalanceClient = tradeBalanceClient;
            _messageProducerService = messageProducerService;
        }

        public async Task UpdateTradeBalanceAsync(string customerApiId, int? portfolioId)
        {
            _logger.LogInformation(
                "Updating trade balance for customer with api id {customerApiId}",
                customerApiId);

            var featureFlagIsEnabledForCustomer = await _featureFlagClient.IsFeatureFlagEnabledForCustomerAsync(
                featureFlag: BreakingChangeEnableUnifiedAccount,
                customerApiId: customerApiId);

            if (featureFlagIsEnabledForCustomer)
            {
                _logger.LogInformation(
                    "Unified account breaking change feature flag is enabled for customer api id: {customerApiId}",
                    customerApiId);

                await _tradeBalanceClient.UpdateAccountingBalanceAsync(customerApiId);
            }
            else
            {
                await ProduceBalanceUpdateMessageAsync(portfolioId, customerApiId);
            }
        }

        private async Task ProduceBalanceUpdateMessageAsync(int? portfolioId, string? customerApiId = null)
        {
            if (portfolioId is null)
            {
                _logger.LogWarning("Unable to create balance update message because portfolio id is null");
                return;
            }

            await _messageProducerService
                .ProduceBalanceUpdateMessageAsync((int)portfolioId, customerApiId);

            _logger.LogInformation(
                "Balance update message has been produced for the portfolio {portfolioId}",
                portfolioId);
        }
    }
}
