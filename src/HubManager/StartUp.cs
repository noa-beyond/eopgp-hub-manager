using Application;
using Infrastructure;
using Infrastructure.Messaging.Kafka.Config;
using Infrastructure.Percistance.Config;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Diagnostics;
using System.Reflection;

namespace HubManager
{
    public static class HubManagerTelemetry
    {
        // Use this in your code to create spans:
        public static readonly ActivitySource ActivitySource = new("NoaHubManager");
    }

    public class Startup
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<Startup> _logger;
        private const string ApplicationName = "NoaHubManager";

        public Startup(IConfiguration configuration, ILogger<Startup> logger)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void ConfigureServices(IServiceCollection services)
        {
            _logger.LogInformation("Starting service configuration for {ApplicationName}", ApplicationName);

            ConfigureLogging(services);
            ConfigureOpenTelemetry(services);
            ConfigureCoreServices(services);
            ConfigureOptions(services);
            ConfigureHostedServices(services);

            _logger.LogInformation("Service configuration completed. Registered services: {ServiceCount}", services.Count);
        }

        private void ConfigureLogging(IServiceCollection services)
        {
            services.AddLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddConsole(o =>
                {
                    o.IncludeScopes = true;
                    o.TimestampFormat = "yyyy-MM-dd HH:mm:ss.fff ";
                });

                // OpenTelemetry log exporter (OTLP)
                logging.AddOpenTelemetry(o =>
                {
                    o.IncludeScopes = true;
                    o.IncludeFormattedMessage = true;
                    o.ParseStateValues = true;

                    // resource attrs align with traces/metrics
                    o.SetResourceBuilder(CreateResourceBuilder());

                    var endpoint = _configuration.GetValue<string>("OpenTelemetry:Otlp:Endpoint");
                    var proto = _configuration.GetValue<string>("OpenTelemetry:Otlp:Protocol", "grpc");

                    if (!string.IsNullOrWhiteSpace(endpoint))
                    {
                        if (string.Equals(proto, "http/protobuf", StringComparison.OrdinalIgnoreCase))
                            o.AddOtlpExporter(opt => { opt.Endpoint = new Uri(endpoint); opt.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf; });
                        else
                            o.AddOtlpExporter(opt => { opt.Endpoint = new Uri(endpoint); opt.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc; });
                    }

                    if (_configuration.GetValue<bool>("OpenTelemetry:ConsoleExport"))
                        o.AddConsoleExporter();
                });

                var min = _configuration.GetValue("Logging:LogLevel:Default", LogLevel.Information);
                logging.SetMinimumLevel(min);
            });
        }

        private void ConfigureOpenTelemetry(IServiceCollection services)
        {
            var serviceName = _configuration["OpenTelemetry:ServiceName"] ?? _configuration["ApplicationName"] ?? ApplicationName;
            var serviceVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0";

            // TRACES + METRICS
            services.AddOpenTelemetry()
                .ConfigureResource(rb => rb.AddService(serviceName, serviceVersion))
                .WithTracing(builder =>
                {
                    builder
                        .SetResourceBuilder(CreateResourceBuilder())
                        .AddSource(ApplicationName)                   // your Startup traces
                        .AddSource(HubManagerTelemetry.ActivitySource.Name) // your custom spans
                        .AddHttpClientInstrumentation();

                    var endpoint = _configuration.GetValue<string>("OpenTelemetry:Otlp:Endpoint");
                    var proto = _configuration.GetValue<string>("OpenTelemetry:Otlp:Protocol", "grpc");

                    if (!string.IsNullOrWhiteSpace(endpoint))
                    {
                        if (string.Equals(proto, "http/protobuf", StringComparison.OrdinalIgnoreCase))
                            builder.AddOtlpExporter(opt => { opt.Endpoint = new Uri(endpoint); opt.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf; });
                        else
                            builder.AddOtlpExporter(opt => { opt.Endpoint = new Uri(endpoint); opt.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc; });
                    }

                    if (_configuration.GetValue<bool>("OpenTelemetry:ConsoleExport"))
                        builder.AddConsoleExporter();
                })
                .WithMetrics(builder =>
                {
                    builder
                        .SetResourceBuilder(CreateResourceBuilder())
                        .AddRuntimeInstrumentation()  // GC, allocations
                        .AddMeter("System.Runtime"); // Adds runtime metrics including CPU, memory, threads

                    var endpoint = _configuration.GetValue<string>("OpenTelemetry:Otlp:Endpoint");
                    var proto = _configuration.GetValue<string>("OpenTelemetry:Otlp:Protocol", "grpc");

                    if (!string.IsNullOrWhiteSpace(endpoint))
                    {
                        if (string.Equals(proto, "http/protobuf", StringComparison.OrdinalIgnoreCase))
                            builder.AddOtlpExporter(opt => { opt.Endpoint = new Uri(endpoint); opt.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf; });
                        else
                            builder.AddOtlpExporter(opt => { opt.Endpoint = new Uri(endpoint); opt.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc; });
                    }

                    if (_configuration.GetValue<bool>("OpenTelemetry:ConsoleExport"))
                        builder.AddConsoleExporter();
                });

            _logger.LogInformation("OpenTelemetry configured for {ServiceName} v{ServiceVersion}", serviceName, serviceVersion);
        }

        private static ResourceBuilder CreateResourceBuilder()
        {
            // You can enrich with deployment.environment, service.instance.id, etc.
            return ResourceBuilder.CreateDefault()
                .AddService(
                    serviceName: "NoaHubManager",
                    serviceVersion: Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0")
                .AddAttributes(new Dictionary<string, object>
                {
                    ["host.name"] = Environment.MachineName
                });
        }

        private void ConfigureCoreServices(IServiceCollection services)
        {
            services.AddApplicationServices();
            services.AddInfrastructureServices();
        }

        private void ConfigureOptions(IServiceCollection services)
        {
            var connectionStringsSection = _configuration.GetSection(nameof(ConnectionStringsDb));
            if (!connectionStringsSection.Exists())
                _logger.LogWarning("ConnectionStringsDb configuration section not found");
            services.Configure<ConnectionStringsDb>(connectionStringsSection);

            var kafkaSection = _configuration.GetSection(nameof(KafkaConfiguration));
            if (!kafkaSection.Exists())
                _logger.LogWarning("KafkaConfiguration section not found in configuration");
            else
                _logger.LogDebug("Kafka brokers: {Brokers}", kafkaSection.GetValue<string>("Brokers", "Not configured"));
            services.Configure<KafkaConfiguration>(kafkaSection);
        }

        private void ConfigureHostedServices(IServiceCollection services)
        {
            services.AddHostedService<MessagesBackgroundService>();
        }

        public void Configure(IHostApplicationLifetime appLifetime, ILogger<Startup> logger)
        {
            appLifetime.ApplicationStarted.Register(() => OnApplicationStarted(logger));
            appLifetime.ApplicationStopping.Register(() => OnApplicationStopping(logger));
            appLifetime.ApplicationStopped.Register(() => OnApplicationStopped(logger));
        }

        private void OnApplicationStarted(ILogger<Startup> logger)
        {
            logger.LogInformation("{App} started at {Utc} (PID {Pid})",
                ApplicationName, DateTime.UtcNow, Environment.ProcessId);
        }

        private void OnApplicationStopping(ILogger<Startup> logger)
        {
            logger.LogInformation("{App} stopping at {Utc}", ApplicationName, DateTime.UtcNow);
        }

        private void OnApplicationStopped(ILogger<Startup> logger)
        {
            logger.LogInformation("{App} stopped at {Utc}", ApplicationName, DateTime.UtcNow);
        }
    }
}
