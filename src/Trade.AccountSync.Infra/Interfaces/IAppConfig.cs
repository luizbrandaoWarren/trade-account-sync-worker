using System.Collections.Generic;

namespace Warren.Trade.Risk.Infra.Interfaces
{
    public interface IAppConfig
    {
        string GetConfig(string configKey);

        Dictionary<string, T> GetPairs<T>(string configKey);

        T GetConfig<T>(string configKey);

        T? TryGetConfig<T>(string configKey) where T : struct;
    }
}
