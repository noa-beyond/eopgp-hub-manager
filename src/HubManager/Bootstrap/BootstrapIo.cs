using System.Text;

namespace HubManager.Bootstrap;

public static class BootstrapIo
{
    public static void EnsureDirectoriesExist(BootstrapContext ctx)
    {
        try
        {
            Directory.CreateDirectory(ctx.ConfigDir);
            Directory.CreateDirectory(ctx.LogsPath);
            Console.WriteLine($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff}] Ensured directories - Config: {ctx.ConfigDir}, Logs: {ctx.LogsPath}");
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to create required directories (Config='{ctx.ConfigDir}', Logs='{ctx.LogsPath}').", ex);
        }
    }

    public static void EnsureDefaultAppSettings(BootstrapContext ctx)
    {
        if (File.Exists(ctx.ConfigFile)) return;

        var candidate = Path.Combine(ctx.BasePath, "defaults", "appsettings.default.json");
        if (File.Exists(candidate))
        {
            File.Copy(candidate, ctx.ConfigFile, overwrite: true);
            Console.WriteLine($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff}] Copied default config from {candidate}");
            return;
        }

        var defaultJson = /* language=json */ """
        {
          "Serilog": {
            "MinimumLevel": {
              "Default": "Information",
              "Override": { "Microsoft": "Warning", "System": "Warning" }
            },
            "WriteTo": [
              { "Name": "Console" },
              { "Name": "File", "Args": { "path": "NoaHubManager-.log", "rollingInterval": "Day", "shared": true } }
            ],
            "Enrich": [ "FromLogContext" ]
          },
          "KafkaConfiguration": { },
          "ConnectionStringsDb": { }
        }
        """;

        File.WriteAllText(ctx.ConfigFile, defaultJson, Encoding.UTF8);
        Console.WriteLine($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff}] Created minimal config at {ctx.ConfigFile}");
    }
}
