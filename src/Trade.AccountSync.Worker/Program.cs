using Microsoft.Extensions.Hosting;
using Serilog;
using Warren.Core.Extensions.Hosting;
using Warren.Trade.Risk.ClientV2;
using Warren.Trade.Risk.ClientV2.Logging;

public class Program
{
    public static void Main(string[] args) => CreateHostBuilder(args).Build().Run();

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .UseSerilog((hostContext, loggerConfiguration) => loggerConfiguration.Configure(hostContext.Configuration, hostContext.HostingEnvironment), writeToProviders: true)
            .ConfigureServices((hostContext, services) => services.UseStartup<Startup>(hostContext.Configuration));
}