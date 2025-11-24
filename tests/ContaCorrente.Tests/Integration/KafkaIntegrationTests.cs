using System.Text.Json;
using Confluent.Kafka;
using FluentAssertions;
using Xunit;

namespace ContaCorrente.Tests.Integration;

public class KafkaIntegrationTests : IDisposable
{
    private readonly IProducer<string, string> _producer;
    private readonly IConsumer<string, string> _consumer;
    private const string BootstrapServers = "localhost:9092";

    public KafkaIntegrationTests()
    {
        var producerConfig = new ProducerConfig
        {
            BootstrapServers = BootstrapServers
        };

        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = BootstrapServers,
            GroupId = "test-group",
            AutoOffsetReset = AutoOffsetReset.Earliest
        };

        _producer = new ProducerBuilder<string, string>(producerConfig).Build();
        _consumer = new ConsumerBuilder<string, string>(consumerConfig).Build();
    }

    [Fact(Skip = "Requires Kafka running")]
    public async Task DevePublicarEConsumirEventoDeDeposito()
    {
        // Arrange
        var topic = "depositos";
        var evento = new
        {
            IdContaCorrente = "teste-123",
            IdMovimento = Guid.NewGuid().ToString(),
            Valor = 100.50m,
            DataMovimento = DateTime.Now
        };

        var mensagem = JsonSerializer.Serialize(evento);

        // Act - Publish
        var deliveryResult = await _producer.ProduceAsync(topic, new Message<string, string>
        {
            Key = evento.IdMovimento,
            Value = mensagem
        });

        // Assert - Publish
        deliveryResult.Status.Should().Be(PersistenceStatus.Persisted);

        // Act - Consume
        _consumer.Subscribe(topic);
        var consumeResult = _consumer.Consume(TimeSpan.FromSeconds(10));

        // Assert - Consume
        consumeResult.Should().NotBeNull();
        consumeResult.Message.Value.Should().Be(mensagem);
    }

    [Fact(Skip = "Requires Kafka running")]
    public async Task DevePublicarEConsumirEventoDeSaque()
    {
        // Arrange
        var topic = "saques";
        var evento = new
        {
            IdContaCorrente = "teste-456",
            IdMovimento = Guid.NewGuid().ToString(),
            Valor = 50.00m,
            DataMovimento = DateTime.Now
        };

        var mensagem = JsonSerializer.Serialize(evento);

        // Act
        await _producer.ProduceAsync(topic, new Message<string, string>
        {
            Key = evento.IdMovimento,
            Value = mensagem
        });

        _consumer.Subscribe(topic);
        var consumeResult = _consumer.Consume(TimeSpan.FromSeconds(10));

        // Assert
        consumeResult.Should().NotBeNull();
        var eventoRecebido = JsonSerializer.Deserialize<dynamic>(consumeResult.Message.Value);
        eventoRecebido.Should().NotBeNull();
    }

    public void Dispose()
    {
        _producer?.Dispose();
        _consumer?.Close();
        _consumer?.Dispose();
    }
}