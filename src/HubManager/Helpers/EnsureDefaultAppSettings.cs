using Microsoft.Extensions.Configuration;
using Serilog;
using System.Text;

namespace HubManager.Helpers;
public static class HelperDefaults
{
    public static void EnsureDefaultAppSettings(string fullPath)
    {
        var dir = Path.GetDirectoryName(fullPath)!;
        Directory.CreateDirectory(dir);

        if (File.Exists(fullPath)) return;

        var defaultJson = """
    {
      "ApplicationName": "NoaHubManager",
      "Environment": "Production",
      "Logging": {
        "LogLevel": {
          "Default": "Information",
          "Microsoft": "Warning",
          "Microsoft.Hosting.Lifetime": "Information"
        }
      },
      "Serilog": {
        "MinimumLevel": {
          "Default": "Information",
          "Override": {
            "Microsoft": "Warning",
            "System": "Warning"
          }
        },
        "Enrich": [ "FromLogContext", "WithMachineName", "WithProcessId", "WithThreadId" ],
        "Properties": {
          "Application": "NoaHubManager",
          "Environment": "%DOTNET_ENVIRONMENT%"
        },
        "WriteTo": [
          { "Name": "Console" },
          {
            "Name": "File",
            "Args": {
              "path": "logs/NoaHubManager-.log",
              "rollingInterval": "Day",
              "retainedFileCountLimit": 14,
              "fileSizeLimitBytes": 52428800,
              "rollOnFileSizeLimit": true,
              "shared": true
            }
          }
        ]
      },
      "OpenTelemetry": {
        "ServiceName": "NoaHubManager",
        "ServiceVersion": "1.0.0",
        "Otlp": { "Endpoint": "http://localhost:4317", "Protocol": "grpc" },
        "ConsoleExport": false
      },
      "KafkaConfiguration": {
        "Brokers": "localhost:9092",
        "ClientId": "noa-hub-manager"
      },
      "ConnectionStringsDb": {
        "DefaultConnection": "Host=localhost;Port=5432;Database=noa;Username=noa;Password=noa"
      }
    }
    """;

        File.WriteAllText(fullPath, defaultJson, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        Console.WriteLine($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff}] Seeded {fullPath}");
    }

    public static ILogger CreateLoggerFromConfiguration(IConfiguration config, string appName)
    {
        try
        {
            var serilogSection = config.GetSection("Serilog");
            if (serilogSection.Exists())
            {
                return new LoggerConfiguration()
                    .ReadFrom.Configuration(config)
                    .Enrich.FromLogContext()
                    .Enrich.WithProperty("Application", appName)
                    .CreateLogger();
            }
        }
        catch { /* fall back */ }

        var logsDir = Path.Combine(AppContext.BaseDirectory, "logs");
        Directory.CreateDirectory(logsDir);

        return new LoggerConfiguration()
            .MinimumLevel.Information()
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Application", appName)
            .WriteTo.Console(outputTemplate:
                "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff} {Level:u3}] {Application} {SourceContext} {Message:lj}{NewLine}{Exception}")
            .WriteTo.File(
                path: Path.Combine(logsDir, $"{appName}-.log"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 14,
                fileSizeLimitBytes: 50 * 1024 * 1024,
                rollOnFileSizeLimit: true,
                shared: true,
                outputTemplate:
                "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff} {Level:u3}] {Application} {SourceContext} {Message:lj}{NewLine}{Exception}")
            .CreateLogger();
    }

}