using Application.Interfaces;
using Domain.ValueObjects.ChDM;
using Domain.ValueObjects.Gateway;
using Domain.ValueObjects.Stac;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HubManager
{
    public class MessagesBackgroundService(
        IKafkaService<GatewayMessage> gatewayKafkaService,
        IKafkaService<ChDetectionMappingConsumerMessage> chDetectionMappingKafkaService,
        IKafkaService<StacConsumerMessage> stacKafkaService,
        ILogger<MessagesBackgroundService> logger)
        : BackgroundService
    {
        private const string ServiceName = nameof(MessagesBackgroundService);

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var scope = logger.BeginScope("Service: {ServiceName}", ServiceName);

            logger.LogInformation("Initializing Kafka services startup sequence");

            try
            {
                // Start each service individually with detailed logging
                await StartKafkaServiceAsync("Gateway", () => gatewayKafkaService.Start(stoppingToken), stoppingToken);
                await StartKafkaServiceAsync("Harvester", () => chDetectionMappingKafkaService.Start(stoppingToken), stoppingToken);
                await StartKafkaServiceAsync("STAC", () => stacKafkaService.Start(stoppingToken), stoppingToken);

                logger.LogInformation("All Kafka services started successfully. Total services: {ServiceCount}", 3);

                // Keep the service running until cancellation is requested
                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                logger.LogInformation("Kafka services startup was canceled");
                throw;
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, "Critical error occurred during Kafka services startup. Service will terminate");
                throw;
            }
        }

        private async Task StartKafkaServiceAsync(string serviceName, Action startAction, CancellationToken cancellationToken)
        {
            logger.LogDebug("Starting {ServiceType} Kafka service...", serviceName);

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                await Task.Run(startAction, cancellationToken);
                stopwatch.Stop();

                logger.LogInformation("{ServiceType} Kafka service started successfully in {ElapsedMs}ms",
                    serviceName, stopwatch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                logger.LogError(ex, "Failed to start {ServiceType} Kafka service after {ElapsedMs}ms. Error: {ErrorMessage}",
                    serviceName, stopwatch.ElapsedMilliseconds, ex.Message);
                throw;
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            using var scope = logger.BeginScope("Service: {ServiceName}", ServiceName);

            logger.LogInformation("Initiating graceful shutdown of Kafka services");

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var errors = new List<Exception>();

            // Stop services in reverse order for graceful shutdown
            await StopKafkaServiceAsync("STAC", () => stacKafkaService.Stop(), errors);
            await StopKafkaServiceAsync("Harvester", () => chDetectionMappingKafkaService.Stop(), errors);
            await StopKafkaServiceAsync("Gateway", () => gatewayKafkaService.Stop(), errors);

            stopwatch.Stop();

            if (errors.Count > 0)
            {
                logger.LogWarning("Kafka services shutdown completed with {ErrorCount} errors in {ElapsedMs}ms",
                    errors.Count, stopwatch.ElapsedMilliseconds);

                foreach (var error in errors)
                {
                    logger.LogWarning(error, "Shutdown error: {ErrorMessage}", error.Message);
                }
            }
            else
            {
                logger.LogInformation("All Kafka services stopped gracefully in {ElapsedMs}ms",
                    stopwatch.ElapsedMilliseconds);
            }

            await base.StopAsync(cancellationToken);
        }

        private async Task StopKafkaServiceAsync(string serviceName, Action stopAction, List<Exception> errors)
        {
            logger.LogDebug("Stopping {ServiceType} Kafka service...", serviceName);

            try
            {
                await Task.Run(stopAction);
                logger.LogDebug("{ServiceType} Kafka service stopped successfully", serviceName);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Error stopping {ServiceType} Kafka service: {ErrorMessage}",
                    serviceName, ex.Message);
                errors.Add(ex);
            }
        }
    }
}