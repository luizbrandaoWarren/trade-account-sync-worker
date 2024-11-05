using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis.Extensions.Core;
using StackExchange.Redis.Extensions.Core.Abstractions;
using StackExchange.Redis.Extensions.Core.Configuration;
using StackExchange.Redis.Extensions.Core.Implementations;
using StackExchange.Redis.Extensions.Newtonsoft;

namespace Warren.Trade.Risk.ClientV2.Extensions
{
    public static class RedisExtensions
    {
        public static void AddRedis(
            this IServiceCollection services, 
            IConfiguration configuration)
        {
            services.AddSingleton(configuration.GetSection("Redis").Get<RedisConfiguration>());
            services.AddSingleton<IRedisCacheClient, RedisCacheClient>();
            services.AddSingleton<IRedisCacheConnectionPoolManager, RedisCacheConnectionPoolManager>();
            services.AddSingleton<ISerializer, NewtonsoftSerializer>();
        }
    }
}
