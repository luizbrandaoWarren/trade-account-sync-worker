using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Warren.Core.Messaging.AccountIntegration.Contracts.Models;
using Warren.Core.Messaging.Consumers;
using Warren.Core.Messaging.Hosting.Providers.Kafka;
using Warren.Core.Messaging.Idempotency.Providers.Redis;
using Warren.Core.Messaging.Providers.Kafka.Consumers;
using Warren.Core.Messaging.Providers.Kafka.Models;
using Warren.Core.Messaging.Providers.Kafka.Producers;
using Warren.Core.Messaging.Retry.Providers.Polly;
using Warren.Trade.Risk.ClientV2.Services.Interfaces;

namespace Warren.Trade.Risk.ClientV2.HostedServices
{
    public class SinacorAccountBackgroundService : AsyncKafkaMessageHandlerService<SinacorAccountIntegratedMessage>
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public SinacorAccountBackgroundService(
            ILoggerFactory loggerFactory,
            IHostApplicationLifetime appLifetime,
            KafkaParallelSettings settings,
            IKafkaMessageConsumer<SinacorAccountIntegratedMessage> consumer,
            IRedisMessageIdempotencyHandler<SinacorAccountIntegratedMessage> idempotencyHandler,
            ISimpleMessageRetryHandler retryHandler,
            IKafkaMessageProducer<KafkaRetryQueueMessage> retryProducer,
            IKafkaMessageProducer<KafkaDeadLetterQueueMessage> deadLetterProducer,
            IServiceScopeFactory serviceScopeFactory)
            : base(loggerFactory, appLifetime, settings, consumer, idempotencyHandler, retryHandler, retryProducer, deadLetterProducer)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        protected async override Task ExecuteAsync(
            MessageConsumerResult<SinacorAccountIntegratedMessage> result,
            CancellationToken cancellationToken)
        {
            var message = result.Message;

            using var scope = _serviceScopeFactory.CreateScope();
            using var service = scope.ServiceProvider.GetRequiredService<ICustomerService>();

            await service.UpdateSinacorAccountAsync(message.Customer.ApiId, message.SinacorId);
        }
    }
}
