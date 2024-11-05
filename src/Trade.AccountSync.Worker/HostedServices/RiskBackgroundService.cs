using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Warren.Core.Messaging.Consumers;
using Warren.Core.Messaging.Hosting.Providers.Kafka;
using Warren.Core.Messaging.Idempotency.Providers.Redis;
using Warren.Core.Messaging.Providers.Kafka.Consumers;
using Warren.Core.Messaging.Retry.Providers.Polly;
using Warren.Core.Messaging.Risk.Contracts.Models;
using Warren.Trade.Risk.ClientV2.Services;

namespace Warren.Trade.Risk.ClientV2.HostedServices
{
    public class RiskBackgroundService : AsyncKafkaMessageHandlerService<CreateCustomerRequestMessage>
    {
        private readonly IRequestService _requestService;

        public RiskBackgroundService(ILoggerFactory loggerFactory,
                                     IHostApplicationLifetime appLifetime,
                                     KafkaParallelSettings settings,
                                     IKafkaMessageConsumer<CreateCustomerRequestMessage> consumer,
                                     IRequestService requestService,
                                     IRedisMessageIdempotencyHandler<CreateCustomerRequestMessage> idempotencyHandler,
                                     ISimpleMessageRetryHandler retryHandler)
            : base(loggerFactory, appLifetime, settings, consumer, idempotencyHandler, retryHandler)
        {
            _requestService = requestService;
        }

        protected async override Task ExecuteAsync(MessageConsumerResult<CreateCustomerRequestMessage> result, CancellationToken cancellationToken = default)
        {
            await _requestService.ProcessCustomerRegister(result);
        }
    }
}
