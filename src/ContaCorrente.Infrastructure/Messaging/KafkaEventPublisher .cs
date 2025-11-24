using System.Text.Json;
using Confluent.Kafka;
using ContaCorrente.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ContaCorrente.Infrastructure.Messaging;

public class KafkaEventPublisher : IEventPublisher, IDisposable
{
    private readonly IProducer<string, string> _producer;
    private readonly ILogger<KafkaEventPublisher> _logger;

    public KafkaEventPublisher(IConfiguration configuration, ILogger<KafkaEventPublisher> logger)
    {
        _logger = logger;

        var config = new ProducerConfig
        {
            BootstrapServers = configuration["Kafka:BootstrapServers"] ?? "localhost:9092",
            ClientId = configuration["Kafka:ClientId"] ?? "contacorrente-api",
            Acks = Acks.All,
            EnableIdempotence = true,
            MessageSendMaxRetries = 3,
            RetryBackoffMs = 1000
        };

        _producer = new ProducerBuilder<string, string>(config).Build();

        _logger.LogInformation("Kafka Producer initialized with bootstrap servers: {BootstrapServers}",
            config.BootstrapServers);
    }

    public async Task PublishAsync<TEvent>(string topic, TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : class
    {
        var key = Guid.NewGuid().ToString();
        await PublishAsync(topic, key, @event, cancellationToken);
    }

    public async Task PublishAsync<TEvent>(string topic, string key, TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : class
    {
        try
        {
            var eventJson = JsonSerializer.Serialize(@event, new JsonSerializerOptions
            {
                WriteIndented = false
            });

            var message = new Message<string, string>
            {
                Key = key,
                Value = eventJson,
                Headers = new Headers
                {
                    { "event-type", System.Text.Encoding.UTF8.GetBytes(typeof(TEvent).Name) },
                    { "timestamp", System.Text.Encoding.UTF8.GetBytes(DateTime.UtcNow.ToString("o")) }
                }
            };

            var deliveryResult = await _producer.ProduceAsync(topic, message, cancellationToken);

            _logger.LogInformation(
                "Event {EventType} published to topic {Topic}, partition {Partition}, offset {Offset}",
                typeof(TEvent).Name,
                deliveryResult.Topic,
                deliveryResult.Partition.Value,
                deliveryResult.Offset.Value);
        }
        catch (ProduceException<string, string> ex)
        {
            _logger.LogError(ex, "Error publishing event {EventType} to topic {Topic}", typeof(TEvent).Name, topic);
            throw;
        }
    }

    public void Dispose()
    {
        _producer?.Flush(TimeSpan.FromSeconds(10));
        _producer?.Dispose();
    }
}