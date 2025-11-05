using Confluent.Kafka;
using Domain.Interfaces;
using Infrastructure.Kafka.Config;
using Infrastructure.Messaging.Interfaces;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Infrastructure.Messaging.Kafka.Common
{
    public abstract class BaseKafkaConsumer<T> : IKafkaConsumer<T>, IDisposable
        where T : IKafkaMessage
    {
        private readonly BaseKafkaConfiguration _kafkaSettings;
        private readonly ILogger<BaseKafkaConsumer<T>> _logger;
        private CancellationTokenSource? _cancellationTokenSource;
        private Task? _consumeTask;
        private IConsumer<Ignore, string>? _consumer;
        private bool _disposed;

        public BaseKafkaConsumer(BaseKafkaConfiguration kafkaConfiguration, ILogger<BaseKafkaConsumer<T>> logger)
        {
            _kafkaSettings = kafkaConfiguration ?? throw new ArgumentNullException(nameof(kafkaConfiguration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Init()
        {
            if (_cancellationTokenSource != null)
            {
                _logger.LogWarning("Consumer is already initialized");
                return;
            }

            var config = new ConsumerConfig
            {
                BootstrapServers = _kafkaSettings.BootstrapServers,
                GroupId = _kafkaSettings.GroupId,
                AutoOffsetReset = _kafkaSettings.AutoOffsetReset,
                EnableAutoCommit = _kafkaSettings.EnableAutoCommit,
                SessionTimeoutMs = _kafkaSettings.SessionTimeoutMs,
            };

            try
            {
                _consumer = new ConsumerBuilder<Ignore, string>(config)
                    .SetErrorHandler(HandleKafkaError)
                    .Build();

                _consumer.Subscribe(_kafkaSettings.Topic);
                _cancellationTokenSource = new CancellationTokenSource();

                _logger.LogInformation("Kafka consumer started for topic: {Topic}, GroupId: {GroupId}",
                    _kafkaSettings.Topic, _kafkaSettings.GroupId);

                _consumeTask = Task.Run(() => ConsumeMessages(_cancellationTokenSource.Token),
                    _cancellationTokenSource.Token);

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize Kafka consumer for topic: {Topic}", _kafkaSettings.Topic);
                throw;
            }
        }

        private void HandleKafkaError(IConsumer<Ignore, string> consumer, Error error)
        {
            _logger.LogError("Kafka error occurred: {Reason} (Code: {Code})", error.Reason, error.Code);

            if (error.Code == ErrorCode.UnknownTopicId)
            {
                _logger.LogError("Topic '{Topic}' does not exist or is unavailable", _kafkaSettings.Topic);
                throw new InvalidOperationException($"Topic '{_kafkaSettings.Topic}' does not exist or is unavailable.");
            }

            if (error.IsFatal)
            {
                _logger.LogCritical("Fatal Kafka error encountered: {Reason}", error.Reason);
                throw new InvalidOperationException($"Fatal Kafka error: {error.Reason}");
            }
        }

        private async Task ConsumeMessages(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested && _consumer != null)
            {
                try
                {
                    var consumeResult = _consumer.Consume(TimeSpan.FromMilliseconds(1000));

                    if (consumeResult?.Message?.Value == null)
                        continue;

                    if (consumeResult.IsPartitionEOF)
                    {
                        _logger.LogDebug("Reached end of partition {Partition}", consumeResult.Partition);
                        continue;
                    }

                    await HandleMessage(consumeResult, cancellationToken);
                }
                catch (ConsumeException ex)
                {
                    _logger.LogError(ex, "Error consuming message: {Reason}", ex.Error.Reason);

                    await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("Message consumption cancelled");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error during message consumption");

                    await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
                }
            }

            _logger.LogInformation("Message consumption loop ended");
        }

        private async Task HandleMessage(ConsumeResult<Ignore, string> consumeResult, CancellationToken cancellationToken)
        {
            var messageValue = consumeResult.Message.Value;

            if (messageValue == "Connection test")
            {
                _logger.LogDebug("Received connection test message");
                return;
            }

            _logger.LogInformation("Processing message from topic: {Topic}, partition: {Partition}, offset: {Offset}",
                consumeResult.Topic, consumeResult.Partition, consumeResult.Offset);

            try
            {
                var message = JsonConvert.DeserializeObject<T>(messageValue);
                if (message != null)
                {
                    await ProcessMessage(message);
                    _logger.LogDebug("Successfully processed message from offset: {Offset}", consumeResult.Offset);
                }
                else
                {
                    _logger.LogWarning("Failed to deserialize message: {MessageValue}", messageValue);
                }
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to deserialize message: {MessageValue}", messageValue);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message from offset: {Offset}", consumeResult.Offset);
                throw; // Re-throw to allow for retry logic if needed
            }
        }

        public async Task Stop()
        {
            if (_cancellationTokenSource == null)
            {
                _logger.LogWarning("Consumer is not running");
                return;
            }

            _logger.LogInformation("Stopping Kafka consumer...");

            try
            {
                _cancellationTokenSource.Cancel();

                if (_consumeTask != null)
                {
                    await _consumeTask.ConfigureAwait(false);
                }

                _consumer?.Close();
                _logger.LogInformation("Kafka consumer stopped successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while stopping Kafka consumer");
            }
            finally
            {
                Dispose();
            }
        }

        public abstract Task ProcessMessage(T message);

        public void Dispose()
        {
            if (_disposed)
                return;

            _consumer?.Dispose();
            _cancellationTokenSource?.Dispose();
            _disposed = true;
        }

        void IKafkaConsumer<T>.Stop()
        {
            throw new NotImplementedException();
        }
    }
}