using Amazon.SQS;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Debugging;
using Serilog.Events;
using Serilog.Sinks.Sqs;
using Serilog.Warren.Enrichers;
using Serilog.Warren.Formatters;
using System.Diagnostics;

namespace Warren.Trade.Risk.ClientV2.Logging
{
    public static class LoggerConfigurationExtensions
    {
        public static void Configure(this LoggerConfiguration loggerConfiguration, IConfiguration configuration, IHostEnvironment hostEnvironment)
        {
            if (loggerConfiguration == null)
            {
                throw new ArgumentNullException(nameof(loggerConfiguration));
            }

            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            loggerConfiguration.Enrich.WithEnvironment()
                               .Enrich.WithExtra()
                               .Enrich.WithProperty("service", configuration.GetValue<string>("Logging:ServiceName"))
                               .Enrich.WithProperty("host", Environment.MachineName)
                               .WriteTo.Debug()
                               .WriteTo.Console()
                               .ConfigureLogLevel(configuration)
                               .WriteTo.Conditional(
                                    logEvent => !Debugger.IsAttached,
                                    sinkConfig => sinkConfig.SqsBatchSink(
                                        sqsClient: GetSQSClient(configuration),
                                        sqsQueueUrl: configuration.GetValue<string>("Logging:SQS:QueueUrl"),
                                        batchSizeLimit: configuration.GetValue<int>("Logging:BatchSizeLimit"),
                                        period: configuration.GetValue<TimeSpan>("Logging:Period"),
                                        textFormatter: new WarrenJsonFormatter()
                                    )
                               );
        }

        public static void ConfigureFallbackLogger(this LoggerConfiguration loggerConfiguration, IConfiguration configuration)
        {
            if (loggerConfiguration == null)
            {
                throw new ArgumentNullException(nameof(loggerConfiguration));
            }

            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            if (!configuration.GetValue<bool>("Logging:UseFallbackLogger"))
            {
                return;
            }

            loggerConfiguration.ConfigureLogLevel(configuration)
                               .WriteTo.Debug()
                               .WriteTo.Console();

            var fallbackLogger = loggerConfiguration.CreateLogger();

            SelfLog.Enable(output => fallbackLogger.Error(output));
        }

        public static LoggerConfiguration ConfigureLogLevel(this LoggerConfiguration loggerConfiguration, IConfiguration configuration)
        {
            if (loggerConfiguration == null)
            {
                throw new ArgumentNullException(nameof(loggerConfiguration));
            }

            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            var logLevels = GetLogLevels(configuration);

            if (logLevels?.Any() ?? false)
            {
                if (logLevels.TryGetValue("Default", out var minimumLevel))
                {
                    loggerConfiguration.MinimumLevel.Is(minimumLevel);
                }

                foreach (var logLevel in logLevels.Where(kvp => kvp.Key != "Default"))
                {
                    loggerConfiguration.MinimumLevel.Override(logLevel.Key, logLevel.Value);
                }
            }

            return loggerConfiguration;
        }

        private static IAmazonSQS? GetSQSClient(IConfiguration configuration)
        {
            var options = configuration.GetAWSOptions();

            var client = options?.CreateServiceClient<IAmazonSQS>();

            return client;
        }

        private static Dictionary<string, LogEventLevel>? GetLogLevels(IConfiguration configuration)
        {
            var section = configuration.GetSection("Logging:LogLevel");

            var logLevels = section?.Get<Dictionary<string, string>>();

            if (logLevels?.Any() ?? false)
            {
                return logLevels.Where(kvp => Enum.IsDefined(typeof(LogEventLevel), kvp.Value))
                                .ToDictionary(
                                   kvp => kvp.Key,
                                   kvp => (LogEventLevel)Enum.Parse(typeof(LogEventLevel), kvp.Value)
                                );
            }

            return null;
        }
    }
}
