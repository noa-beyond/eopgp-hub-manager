using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace HubManager.Bootstrap;

public static class HostRunner
{
    public static async Task RunAsync(BootstrapContext ctx, IHost host)
    {
        var cts = new CancellationTokenSource();
        var lifetime = host.Services.GetRequiredService<IHostApplicationLifetime>();

        Console.CancelKeyPress += (sender, e) =>
        {
            Log.Information("Shutdown requested via Ctrl+C");
            e.Cancel = true;
            cts.Cancel();
        };

        lifetime.ApplicationStarted.Register(() =>
        {
            Log.Information("{ApplicationName} started successfully", ctx.ApplicationName);
        });
        lifetime.ApplicationStopping.Register(() =>
        {
            Log.Information("{ApplicationName} is stopping gracefully...", ctx.ApplicationName);
        });
        lifetime.ApplicationStopped.Register(() =>
        {
            Log.Information("{ApplicationName} stopped", ctx.ApplicationName);
        });

        try
        {
            await host.RunAsync(cts.Token);
        }
        catch (OperationCanceledException) when (cts.IsCancellationRequested)
        {
            Log.Information("{ApplicationName} was cancelled", ctx.ApplicationName);
        }
    }
}
