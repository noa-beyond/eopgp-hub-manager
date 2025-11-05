using HubManager.Logging;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace HubManager.Bootstrap;

public static class SerilogConfig
{
    public static IConfiguration WithSerilogPathOverrides(BootstrapContext ctx, IConfiguration original)
    {
        var overrides = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var writeTo = original.GetSection("Serilog:WriteTo").GetChildren().ToList();

        for (int i = 0; i < writeTo.Count; i++)
        {
            var sink = writeTo[i];
            var name = sink.GetValue<string>("Name");
            if (!string.Equals(name, "File", StringComparison.OrdinalIgnoreCase)) continue;

            var relPath = sink.GetSection("Args").GetValue<string>("path")
                       ?? sink.GetSection("Args").GetValue<string>("Path");
            if (string.IsNullOrWhiteSpace(relPath)) continue;

            var abs = Path.IsPathRooted(relPath) ? relPath : Path.Combine(ctx.LogsPath, relPath);
            var absDir = Path.GetDirectoryName(abs);
            if (!string.IsNullOrEmpty(absDir)) Directory.CreateDirectory(absDir);

            overrides[$"Serilog:WriteTo:{i}:Args:path"] = abs;
        }

        if (overrides.Count == 0) return original;

        var cb = new ConfigurationBuilder();
        cb.AddConfiguration(original);
        cb.AddInMemoryCollection(overrides);
        return cb.Build();
    }

    public static void SetupSerilog(BootstrapContext ctx, IConfiguration configuration, Redactor redactor)
    {
        var allowPii = configuration.GetValue("Diagnostics:AllowPII", false);

        var loggerCfg = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .Enrich.WithProperty("ApplicationName", ctx.ApplicationName)
            .Enrich.WithProperty("Environment", ctx.EnvironmentName)
            .Enrich.With(new ClassNameEnricher());

        // Instead of raw values, add masked/safe ones
        var userProp = redactor.MaskUser(Environment.UserName);
        var machineProp = redactor.MaskMachine(Environment.MachineName);
        var pidProp = allowPii ? Environment.ProcessId.ToString() : "<pid-hidden>";

        loggerCfg = loggerCfg
            .Enrich.WithProperty("User", userProp)
            .Enrich.WithProperty("Machine", machineProp)
            .Enrich.WithProperty("ProcessId", pidProp);

        try
        {
            Log.Logger = loggerCfg.CreateLogger();
            Log.Information("Serilog configured successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff}] ERROR: Failed to configure Serilog: {ex.Message}");
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console()
                .WriteTo.File(Path.Combine(ctx.LogsPath, "fallback.log"), rollingInterval: RollingInterval.Day, shared: true)
                .Enrich.With(new ClassNameEnricher())
                .CreateLogger();

            Log.Warning("Using fallback Serilog configuration due to configuration error: {Error}", ex.Message);
        }
    }
}
