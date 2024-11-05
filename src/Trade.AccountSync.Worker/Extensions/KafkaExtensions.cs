using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Warren.Core.Messaging.AccountIntegration.Contracts.Models;
using Warren.Core.Messaging.Hosting.Providers.Kafka;
using Warren.Core.Messaging.Hosting.Providers.Kafka.Extensions;
using Warren.Core.Messaging.Idempotency.Providers.Redis;
using Warren.Core.Messaging.Idempotency.Providers.Redis.Extensions;
using Warren.Core.Messaging.Providers.Kafka.Consumers;
using Warren.Core.Messaging.Providers.Kafka.Extensions;
using Warren.Core.Messaging.Providers.Kafka.Models;
using Warren.Core.Messaging.Providers.Kafka.Producers;
using Warren.Core.Messaging.Retry.Providers.Polly;
using Warren.Core.Messaging.Retry.Providers.Polly.Extensions;
using Warren.Core.Messaging.Risk.Contracts.Models;
using Warren.Core.Messaging.Serialization.Providers.Kafka.SystemTextJson;
using Warren.Core.Messaging.Serialization.Providers.Kafka.SystemTextJson.Extensions;
using Warren.Core.Messaging.Transaction.Contracts.Models;
using Warren.Trade.Risk.ClientV2.HostedServices;
using AccountIntregrationContracts = Warren.Core.Messaging.AccountIntegration.Contracts;
using RiskContracts = Warren.Core.Messaging.Risk.Contracts;

namespace Warren.Trade.Risk.ClientV2.Extensions
{
    public static class KafkaExtensions
    {
        private const string TradeBalanceTopic = "Queuing.Transaction.TradeBalance.Create";
        private const string SinacorIntegratedConsumerGroupId = "Trade.Account.Consumer";

        public static IServiceCollection AddKafkaMessagingConfiguration(
           this IServiceCollection services)
        {
            services.AddKafkaSerializersConfiguration();
            services.AddKafkaRetryHandlerConfiguration();

            services.AddKafkaWorkerConsumerConfiguration<CreateCustomerRequestMessage, RiskBackgroundService>(
                RiskContracts.Resources.TopicResources.CreateCustomerTopic,
                RiskContracts.Resources.GroupIdResources.RiskGroupId,
                message =>
                {
                    return message is CreateCustomerRequestMessage request
                        && !request.Identifier.Equals(Guid.Empty)
                        ? request.Identifier.ToString()
                        : null;
                });

            services.AddKafkaWorkerConsumerConfiguration<SinacorAccountIntegratedMessage, SinacorAccountBackgroundService>(
                AccountIntregrationContracts.Resources.TopicResources.SinacorIntegrated,
                SinacorIntegratedConsumerGroupId,
                message =>
                {
                    return message is SinacorAccountIntegratedMessage request
                        && !request.Identifier.Equals(Guid.Empty)
                        ? request.Identifier.ToString()
                        : null;
                });

            services.AddKafkaProducerConfiguration();

            return services;
        }

        private static void AddKafkaSerializersConfiguration(
                this IServiceCollection services)
        {
            services.AddKafkaSystemTextJsonSerializer<KafkaRetryQueueMessage>();
            services.AddKafkaSystemTextJsonSerializer<KafkaDeadLetterQueueMessage>();
        }

        private static void AddKafkaRetryHandlerConfiguration(
            this IServiceCollection services)
        {
            services.AddSimpleMessageRetryHandler<ISimpleMessageRetryHandler, SimpleMessageRetryHandler>();
        }

        private static void AddKafkaWorkerConsumerConfiguration<TMessage, THostedService>(
          this IServiceCollection services,
          string topic,
          string groupId,
          Func<object, string?> idempotencyIdentifierProvider)
          where TMessage : class
          where THostedService : class, IHostedService
        {
            services.AddKafkaSystemTextJsonSerializer<TMessage>();
            services.AddKafkaPubSubConfiguration<TMessage>(topic, groupId);
            services.AddKafkaIdempotencyHandlerConfiguration<TMessage>(topic, groupId, idempotencyIdentifierProvider);
            services.AddKafkaMessageHandlerConfiguration<THostedService>(topic, groupId);
        }

        private static void AddKafkaPubSubConfiguration<TMessage>(
            this IServiceCollection services,
            string topic,
            string groupId)
            where TMessage : class
        {
            services.AddKafkaMessageConsumer<TMessage>(
                delegate (IServiceProvider provider, KafkaMessageConsumerConfig config)
                {
                    config.WithTopic(topic);
                    config.WithGroupId(groupId);
                    config.WithRetryHandler(provider.GetService<ISimpleMessageRetryHandler>());
                    config.WithMessageDeserializer(provider.GetService<IKafkaSystemTextJsonSerializer<TMessage>>());
                    config.WithDeadLetterProducer(provider.GetService<IKafkaMessageProducer<KafkaDeadLetterQueueMessage>>());
                });

            services.AddKafkaMessageProducer<KafkaDeadLetterQueueMessage>(
                delegate (IServiceProvider provider, KafkaMessageProducerConfig config)
                {
                    config.WithRetryHandler(provider.GetService<ISimpleMessageRetryHandler>());
                    config.WithEnableRetryOnFailure(enableRetryOnFailure: true);
                    config.WithMessageSerializer(provider.GetService<IKafkaSystemTextJsonSerializer<KafkaDeadLetterQueueMessage>>());
                });
        }

        private static void AddKafkaIdempotencyHandlerConfiguration<TMessage>(
            this IServiceCollection services,
            string topic,
            string groupId,
            Func<object, string?> idempotencyIdentifierProvider)
        {
            services.AddRedisMessageIdempotencyHandler<
                IRedisMessageIdempotencyHandler<TMessage>,
                RedisMessageIdempotencyHandler<TMessage>>(
                delegate (RedisMessageIdempotencyHandlerOptions options)
                {
                    options.WithHandlerConfiguration(delegate (RedisMessageIdempotencyHandlerConfig config)
                    {
                        config.WithConsumerName(topic + ".Consumer");
                        config.WithGroupId(groupId);
                        config.WithIdentifierProvider(idempotencyIdentifierProvider);
                    });
                });
        }

        private static void AddKafkaMessageHandlerConfiguration<THostedService>(
            this IServiceCollection services,
            string topic,
            string groupId)
            where THostedService : class, IHostedService
        {
            services.AddKafkaMessageHandlerHostedService<THostedService>(
                delegate (AsyncKafkaMessageHandlerOptions options)
                {
                    options.WithRetryProducerConfiguration(delegate (IServiceProvider provider, KafkaMessageProducerConfig config)
                    {
                        config.WithRetryHandler(provider.GetService<ISimpleMessageRetryHandler>());
                        config.WithMessageSerializer(provider.GetService<IKafkaSystemTextJsonSerializer<KafkaRetryQueueMessage>>());
                    });
                });

            services.AddKafkaRetryMessageHandlerHostedService(
                delegate (AsyncKafkaRetryMessageHandlerOptions options)
                {
                    options.WithRetryConsumerConfiguration(delegate (IServiceProvider provider, KafkaMessageConsumerConfig config)
                    {
                        config.WithTopic(topic + ".RetryQueue");
                        config.WithGroupId(groupId);
                        config.WithRetryHandler(provider.GetService<ISimpleMessageRetryHandler>());
                        config.WithMessageDeserializer(provider.GetService<IKafkaSystemTextJsonSerializer<KafkaRetryQueueMessage>>());
                        config.WithEnableDeadLetterQueue(enableDeadLetterQueue: false);
                    });
                    options.WithSourceProducerConfiguration(delegate (IServiceProvider provider, KafkaMessageProducerConfig config)
                    {
                        config.WithRetryHandler(provider.GetService<ISimpleMessageRetryHandler>());
                    });
                });
        }

        private static void AddKafkaProducerConfiguration(this IServiceCollection services)
        {
            services.AddKafkaSystemTextJsonSerializer<TransactionTradeBalanceMessage>();
            services.AddKafkaMessageProducer<int, TransactionTradeBalanceMessage>(
                delegate (IServiceProvider provider, KafkaMessageProducerConfig config)
                {
                    config.WithTopic(TradeBalanceTopic);
                    config.WithRetryHandler(provider.GetService<ISimpleMessageRetryHandler>());
                    config.WithEnableRetryOnFailure(enableRetryOnFailure: true);
                    config.WithMessageSerializer(provider.GetService<IKafkaSystemTextJsonSerializer<TransactionTradeBalanceMessage>>());
                });

            services.AddKafkaSystemTextJsonSerializer<CreateCustomerRequestMessage>();
            services.AddKafkaMessageProducer<CreateCustomerRequestMessage>(
                delegate (IServiceProvider provider, KafkaMessageProducerConfig config)
                {
                    config.WithTopic(RiskContracts.Resources.TopicResources.CreateCustomerTopic);
                    config.WithRetryHandler(provider.GetService<ISimpleMessageRetryHandler>());
                    config.WithEnableRetryOnFailure(enableRetryOnFailure: true);
                    config.WithMessageSerializer(provider.GetService<IKafkaSystemTextJsonSerializer<CreateCustomerRequestMessage>>());
                });
        }
    }
}
