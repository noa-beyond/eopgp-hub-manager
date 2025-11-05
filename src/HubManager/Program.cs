using HubManager.Bootstrap;
using Serilog;

namespace HubManager;

class Program
{
    private const string ApplicationName = "NoaHubManager";

    static async Task<int> Main(string[] args)
    {
        var ctx = new BootstrapContext(ApplicationName);
        BootstrapBanner.Print(ctx, showBasePath: false);

        try
        {
            BootstrapIo.EnsureDirectoriesExist(ctx);
            BootstrapIo.EnsureDefaultAppSettings(ctx);

            var configuration = ConfigLoader.BuildConfiguration(ctx, args);
            configuration = SerilogConfig.WithSerilogPathOverrides(ctx, configuration);

            var redactor = new Redactor(configuration);

            SerilogConfig.SetupSerilog(ctx, configuration, redactor);
            StartupInfo.LogStartupInformation(ctx, redactor);

            var host = HostFactory.CreateHostBuilder(ctx, args, configuration).Build();
            await HostRunner.RunAsync(ctx, host);

            Log.Information("{ApplicationName} completed successfully", ctx.ApplicationName);
            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff}] FATAL: {ex}");
            Log.Fatal(ex, "Host terminated unexpectedly");
            return 1;
        }
        finally
        {
            Log.Information("Shutting down {ApplicationName}", ctx.ApplicationName);
            await Log.CloseAndFlushAsync();
        }
    }
}
