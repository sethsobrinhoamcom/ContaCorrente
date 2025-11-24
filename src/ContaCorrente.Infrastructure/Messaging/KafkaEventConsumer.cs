using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ContaCorrente.Infrastructure.Messaging;

public class KafkaEventConsumer : BackgroundService
{
    private readonly IConsumer<string, string> _consumer;
    private readonly ILogger<KafkaEventConsumer> _logger;
    private readonly string[] _topics;

    public KafkaEventConsumer(
        IConfiguration configuration,
        ILogger<KafkaEventConsumer> logger)
    {
        _logger = logger;

        var config = new ConsumerConfig
        {
            BootstrapServers = configuration["Kafka:BootstrapServers"] ?? "localhost:9092",
            GroupId = configuration["Kafka:ConsumerGroupId"] ?? "contacorrente-consumer-group",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false,
            EnableAutoOffsetStore = false
        };

        _consumer = new ConsumerBuilder<string, string>(config).Build();

        // Tópicos para consumir
        _topics = configuration.GetSection("Kafka:Topics").Get<string[]>()
            ?? new[] { "depositos", "saques", "transferencias" };

        _logger.LogInformation("Kafka Consumer initialized for topics: {Topics}", string.Join(", ", _topics));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield();
        _consumer.Subscribe(_topics);

        _logger.LogInformation("Kafka Consumer started. Listening to topics: {Topics}", string.Join(", ", _topics));

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var consumeResult = _consumer.Consume(stoppingToken);

                    if (consumeResult?.Message != null)
                    {
                        await ProcessMessageAsync(consumeResult, stoppingToken);

                        // Commit manual após processamento bem-sucedido
                        _consumer.Commit(consumeResult);
                        _consumer.StoreOffset(consumeResult);
                    }
                }
                catch (ConsumeException ex)
                {
                    _logger.LogError(ex, "Error consuming message from Kafka");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing Kafka message");
                }
            }
        }
        finally
        {
            _consumer.Close();
        }
    }

    private async Task ProcessMessageAsync(ConsumeResult<string, string> result, CancellationToken cancellationToken)
    {
        var topic = result.Topic;
        var key = result.Message.Key;
        var value = result.Message.Value;

        _logger.LogInformation(
            "Processing message from topic {Topic}, partition {Partition}, offset {Offset}, key: {Key}",
            topic,
            result.Partition.Value,
            result.Offset.Value,
            key);

        // Extrair tipo de evento dos headers
        var eventTypeHeader = result.Message.Headers.FirstOrDefault(h => h.Key == "event-type");
        var eventType = eventTypeHeader != null
            ? System.Text.Encoding.UTF8.GetString(eventTypeHeader.GetValueBytes())
            : "Unknown";

        // Processar baseado no tópico
        switch (topic)
        {
            case "depositos":
                await ProcessDepositoAsync(value, cancellationToken);
                break;
            case "saques":
                await ProcessSaqueAsync(value, cancellationToken);
                break;
            case "transferencias":
                await ProcessTransferenciaAsync(value, cancellationToken);
                break;
            default:
                _logger.LogWarning("Unknown topic: {Topic}", topic);
                break;
        }

        _logger.LogInformation("Message processed successfully. Event Type: {EventType}", eventType);
    }

    private async Task ProcessDepositoAsync(string messageValue, CancellationToken cancellationToken)
    {
        try
        {
            var evento = JsonSerializer.Deserialize<DepositoRealizadoEventDto>(messageValue);

            if (evento != null)
            {
                _logger.LogInformation(
                    "Depósito processado: Conta {IdConta}, Valor {Valor}, Movimento {IdMovimento}",
                    evento.IdContaCorrente,
                    evento.Valor,
                    evento.IdMovimento);

                // Aqui você pode adicionar lógica adicional:
                // - Enviar notificação por email/SMS
                // - Atualizar cache
                // - Enviar para sistema de analytics
                // - Integrar com outros sistemas

                await Task.CompletedTask;
            }
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Error deserializing deposito event");
            throw;
        }
    }

    private async Task ProcessSaqueAsync(string messageValue, CancellationToken cancellationToken)
    {
        try
        {
            var evento = JsonSerializer.Deserialize<SaqueRealizadoEventDto>(messageValue);

            if (evento != null)
            {
                _logger.LogInformation(
                    "Saque processado: Conta {IdConta}, Valor {Valor}, Movimento {IdMovimento}",
                    evento.IdContaCorrente,
                    evento.Valor,
                    evento.IdMovimento);

                // Lógica adicional para processamento de saque
                // - Verificar limites diários
                // - Alertas de segurança
                // - Notificações

                await Task.CompletedTask;
            }
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Error deserializing saque event");
            throw;
        }
    }

    private async Task ProcessTransferenciaAsync(string messageValue, CancellationToken cancellationToken)
    {
        try
        {
            var evento = JsonSerializer.Deserialize<TransferenciaRealizadaEventDto>(messageValue);

            if (evento != null)
            {
                _logger.LogInformation(
                    "Transferência processada: Origem {Origem}, Destino {Destino}, Valor {Valor}, ID {IdTransferencia}",
                    evento.IdContaCorrenteOrigem,
                    evento.IdContaCorrenteDestino,
                    evento.Valor,
                    evento.IdTransferencia);

                // Lógica adicional para processamento de transferência
                // - Detecção de fraude
                // - Compliance
                // - Notificações para ambas as contas

                await Task.CompletedTask;
            }
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Error deserializing transferencia event");
            throw;
        }
    }

    public override void Dispose()
    {
        _consumer?.Close();
        _consumer?.Dispose();
        base.Dispose();
    }
}

// DTOs para deserialização
internal record DepositoRealizadoEventDto
{
    public string IdContaCorrente { get; init; } = string.Empty;
    public string IdMovimento { get; init; } = string.Empty;
    public decimal Valor { get; init; }
    public DateTime DataMovimento { get; init; }
}

internal record SaqueRealizadoEventDto
{
    public string IdContaCorrente { get; init; } = string.Empty;
    public string IdMovimento { get; init; } = string.Empty;
    public decimal Valor { get; init; }
    public DateTime DataMovimento { get; init; }
}

internal record TransferenciaRealizadaEventDto
{
    public string IdTransferencia { get; init; } = string.Empty;
    public string IdContaCorrenteOrigem { get; init; } = string.Empty;
    public string IdContaCorrenteDestino { get; init; } = string.Empty;
    public decimal Valor { get; init; }
    public decimal Tarifa { get; init; }
    public DateTime DataMovimento { get; init; }
}