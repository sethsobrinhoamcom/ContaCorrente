using ContaCorrente.Domain.Events;
using ContaCorrente.Infrastructure.Messaging;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace ContaCorrente.Tests.Infrastructure;

public class KafkaEventPublisherTests : IDisposable
{
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly Mock<ILogger<KafkaEventPublisher>> _loggerMock;
    private readonly KafkaEventPublisher _publisher;

    public KafkaEventPublisherTests()
    {
        _configurationMock = new Mock<IConfiguration>();
        _loggerMock = new Mock<ILogger<KafkaEventPublisher>>();

        _configurationMock
            .Setup(x => x["Kafka:BootstrapServers"])
            .Returns("localhost:9092");

        _configurationMock
            .Setup(x => x["Kafka:ClientId"])
            .Returns("test-client");

        _publisher = new KafkaEventPublisher(_configurationMock.Object, _loggerMock.Object);
    }

    [Fact(Skip = "Requires Kafka running")]
    public async Task PublishAsync_DevePublicarEventoComSucesso()
    {
        // Arrange
        var evento = new DepositoRealizadoEvent
        {
            IdContaCorrente = "conta-123",
            IdMovimento = "movimento-123",
            Valor = 100.00m,
            DataMovimento = DateTime.Now
        };

        // Act
        Func<Task> act = async () => await _publisher.PublishAsync("depositos", evento);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact(Skip = "Requires Kafka running")]
    public async Task PublishAsync_ComChave_DevePublicarComChaveEspecifica()
    {
        // Arrange
        var evento = new SaqueRealizadoEvent
        {
            IdContaCorrente = "conta-456",
            IdMovimento = "movimento-456",
            Valor = 50.00m,
            DataMovimento = DateTime.Now
        };

        var key = "custom-key-123";

        // Act
        Func<Task> act = async () => await _publisher.PublishAsync("saques", key, evento);

        // Assert
        await act.Should().NotThrowAsync();
    }

    public void Dispose()
    {
        _publisher?.Dispose();
    }
}