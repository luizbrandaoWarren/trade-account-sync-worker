using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;
using Warren.Core.Extensions.Hosting;
using Warren.Core.Messaging.Hosting.Providers.Kafka;
using Warren.Core.Slack.Client.Extensions;
using Warren.Trade.Risk.ClientV2.Clients;
using Warren.Trade.Risk.ClientV2.Clients.Interfaces;
using Warren.Trade.Risk.ClientV2.Extensions;
using Warren.Trade.Risk.ClientV2.Logging;
using Warren.Trade.Risk.ClientV2.Services;
using Warren.Trade.Risk.ClientV2.Services.Interfaces;
using Warren.Trade.Risk.Infra;
using Warren.Trade.Risk.Infra.Interfaces;
using Warren.Trade.RiskV2.Client;

namespace Warren.Trade.Risk.ClientV2
{
    [ExcludeFromCodeCoverage]
    public class Startup : IStartup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            var parallelSettings = Configuration.GetSection("Parallel")?.Get<KafkaParallelSettings>() ?? KafkaParallelSettings.New();
            services.AddSingleton(parallelSettings);

            services.AddSlackClient(serviceLifetime: ServiceLifetime.Singleton);

            var loggerConfiguration = LoggerConfigurationFactory.New();
            loggerConfiguration.ConfigureFallbackLogger(Configuration);

            services.AddTransient<IAppConfig, AppConfig>();

            services.AddTransient<IResilientlyHttpClient, ResilientlyHttpClient>();

            services.AddTransient<IRequestService, RequestService>();

            services.AddTransient<ICustomerClient, CustomerClient>();

            services.AddTransient<INotificationService, NotificationService>();

            services.AddTransient<IAccountRiskRegisterService, AccountRiskRegisterService>();

            services.AddTransient<IAccountRiskUpdaterService, AccountRiskUpdaterService>();

            services.AddTransient<IHomebrokerUserRegisterService, HomebrokerUserRegisterService>();

            services.AddTransient<ISuitabilityProfileRegisterService, SuitabilityProfileRegisterService>();

            services.AddTransient<IRiskGroupParser, RiskGroupParser>();

            services.AddTransient<INelogicaClient, NelogicaClient>();

            services.AddTransient<IMessageProducerService, MessageProducerService>();

            services.AddTransient<ITradeCustomerPersistenceService, TradeCustomerPersistenceService>();

            services.AddTransient<ITradeBalanceUpdater, TradeBalanceUpdater>();

            services.AddTransient<ISinacorAccountUpdateService, SinacorAccountUpdateService>();

            services.AddTransient<ICacheManagerService, CacheManagerService>();

            services.AddTransient<ITradeCacheService, TradeCacheService>();

            services.AddTransient<ICustomerService, CustomerService>();

            services.AddTransient<ITradeRlpService, TradeRlpService>();

            services.AddKafkaMessagingConfiguration();

            services.AddRedis(Configuration);

            services.AddTradeBalanceClient(Configuration);

            services.AddTradeOrdersClient(Configuration);

            services.AddFeatureFlagClient(Configuration);

            services.AddTradePositionsClient(Configuration);

            services.AddRlpClient(Configuration);
        }
    }
}
