using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;
using Warren.Core.FeatureFlag.Client.DependencyInjection;
using Warren.Core.Http.Client.Configuration;
using Warren.Core.Http.Client.DependencyInjection;
using Warren.Trade.Risk.ClientV2.Clients;
using Warren.Trade.Risk.ClientV2.Clients.Interfaces;

namespace Warren.Trade.Risk.ClientV2.Extensions
{
    [ExcludeFromCodeCoverage]
    public static class ClientConfigurationExtensions
    {
        public static void AddTradeBalanceClient(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var clientConfiguration = configuration
               .GetSection("TradeBalanceClientConfiguration")
               .Get<HttpClientConfiguration>();

            services.AddHttpClient<ITradeBalanceClient, TradeBalanceClient>(clientConfiguration);
        }

        public static void AddTradeOrdersClient(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var clientConfiguration = configuration
               .GetSection("TradeOrdersClientConfiguration")
               .Get<HttpClientConfiguration>();

            services.AddHttpClient<ITradeOrdersClient, TradeOrdersClient>(clientConfiguration);
        }

        public static void AddRlpClient(
           this IServiceCollection services,
           IConfiguration configuration)
        {
            var clientConfiguration = configuration
               .GetSection("RlpClientConfiguration")
               .Get<HttpClientConfiguration>();

            services.AddHttpClient<IRlpClient, RlpClient>(clientConfiguration);
        }

        public static void AddTradePositionsClient(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var clientConfiguration = configuration
               .GetSection("TradePositionsClientConfiguration")
               .Get<HttpClientConfiguration>();

            services.AddHttpClient<ITradePositionsClient, TradePositionsClient>(clientConfiguration);
        }

        public static void AddFeatureFlagClient(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var section = configuration.GetSection("FeatureFlagClientConfiguration");

            services.AddFeatureFlagClient(
                baseUri: section.GetValue<string>("BaseUrl"),
                maximumRetryAttempts: section.GetValue<int>("MaxRequestAttempt"),
                maximumConsecutiveFailures: section.GetValue<int>("MaxConsecutiveFailures"),
                maximumCircuitBreakerWaitingTime: section.GetValue<int>("MaxCircuitBreakerWaitingTime"),
                timeout: section.GetValue<TimeSpan>("Timeout"));
        }
    }
}
