using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace HubManager.Bootstrap;

public static class HostFactory
{
    public static IHostBuilder CreateHostBuilder(BootstrapContext ctx, string[] args, IConfiguration configuration)
    {
        return Host.CreateDefaultBuilder(args)
            .UseContentRoot(ctx.BasePath)
            .ConfigureAppConfiguration((hostingContext, config) =>
            {
                config.Sources.Clear();
                config.AddConfiguration(configuration);
            })
            .UseSerilog()
            .ConfigureServices((hostingContext, services) =>
            {
                try
                {
                    using var loggerFactory = LoggerFactory.Create(builder => builder.AddSerilog());
                    var startupLogger = loggerFactory.CreateLogger<Startup>();

                    var startup = new Startup(configuration, startupLogger);
                    startup.ConfigureServices(services);

                    Log.Debug("Services configured successfully via Startup class");
                }
                catch (Exception ex)
                {
                    Log.Fatal(ex, "Failed to configure services");
                    throw;
                }
            })
            .UseConsoleLifetime();
    }
}
