using Serilog;

namespace Warren.Trade.Risk.ClientV2.Logging
{
    public static class LoggerConfigurationFactory
    {
        public static LoggerConfiguration New() => new();
    }
}
