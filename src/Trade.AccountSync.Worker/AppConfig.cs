using Microsoft.Extensions.Configuration;
using Warren.Trade.Risk.Infra.Interfaces;

namespace Warren.Trade.RiskV2.Client
{
    public class AppConfig : IAppConfig
    {
        private readonly IConfiguration _configuration;

        public AppConfig(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GetConfig(string configKey)
            => _configuration.GetSection(configKey).Value;

        public T GetConfig<T>(string configKey)
        {
            try
            {
                return (T)Convert.ChangeType(GetConfig(configKey), typeof(T));
            }
            catch (Exception ex)
            {
                throw new Exception($"Error on convert value from configKey '{configKey}' in '{typeof(T).Name}'", ex);
            }
        }

        public T? TryGetConfig<T>(string configKey) where T : struct
        {
            try
            {
                var value = GetConfig(configKey);

                return string.IsNullOrEmpty(value)
                        ? default(T?)
                        : (T)Convert.ChangeType(value, typeof(T));
            }
            catch (Exception ex)
            {
                throw new Exception($"Error on convert value from configKey '{configKey}' in '{typeof(T).Name}'", ex);
            }
        }

        public Dictionary<string, T> GetPairs<T>(string configKey) => throw new NotImplementedException();
    }
}
