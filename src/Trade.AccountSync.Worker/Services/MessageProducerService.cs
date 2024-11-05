using Warren.Core.Messaging.Providers.Kafka.Producers;
using Warren.Core.Messaging.Transaction.Contracts.Models;
using Warren.Trade.Risk.ClientV2.Services.Interfaces;

namespace Warren.Trade.Risk.ClientV2.Services
{
    public class MessageProducerService : IMessageProducerService
    {
        private readonly IKafkaMessageProducer<int, TransactionTradeBalanceMessage> _balanceUpdateMessageProducer;

        public MessageProducerService(
            IKafkaMessageProducer<int, TransactionTradeBalanceMessage> balanceUpdateMessageProducer)
        {
            _balanceUpdateMessageProducer = balanceUpdateMessageProducer;
        }
        public async Task ProduceBalanceUpdateMessageAsync(int portfolioId, string? customerApiId = null)
        {
            var message = new TransactionTradeBalanceMessage
            {
                Identifier = Guid.NewGuid(),
                PortfolioId = portfolioId,
                ApiId = customerApiId
            };

            await _balanceUpdateMessageProducer.ProduceMessageAsync(
                portfolioId,
                message);
        }
    }
}
