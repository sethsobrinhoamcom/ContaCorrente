using System.Text.Json;
using Confluent.Kafka;
using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ContaCorrente.Tarifas.Models;

namespace ContaCorrente.Tarifas.Services;

public class TarifaConsumerService : BackgroundService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<TarifaConsumerService> _logger;
    private readonly IConsumer<string, string> _consumer;
    private readonly IProducer<string, string> _producer;
    private readonly string _connectionString;
    private readonly decimal _valorTarifa;

    public TarifaConsumerService(
        IConfiguration configuration,
        ILogger<TarifaConsumerService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Data Source=contacorrente.db";
        _valorTarifa = decimal.Parse(configuration["Tarifa:ValorTransferencia"] ?? "2.00");

        // Configurar Consumer
        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = configuration["Kafka:BootstrapServers"] ?? "localhost:9092",
            GroupId = configuration["Kafka:ConsumerGroupId"] ?? "tarifas-consumer-group",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false
        };
        _consumer = new ConsumerBuilder<string, string>(consumerConfig).Build();

        // Configurar Producer
        var producerConfig = new ProducerConfig
        {
            BootstrapServers = configuration["Kafka:BootstrapServers"] ?? "localhost:9092",
            ClientId = "tarifas-producer",
            Acks = Acks.All,
            EnableIdempotence = true
        };
        _producer = new ProducerBuilder<string, string>(producerConfig).Build();

        _logger.LogInformation("TarifaConsumerService inicializado. Valor da tarifa: R$ {ValorTarifa}", _valorTarifa);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _consumer.Subscribe("transferencias");
        _logger.LogInformation("Consumer inscrito no tópico 'transferencias'");

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var consumeResult = _consumer.Consume(stoppingToken);

                    if (consumeResult?.Message != null)
                    {
                        await ProcessarTransferenciaAsync(consumeResult, stoppingToken);

                        _consumer.Commit(consumeResult);
                        _consumer.StoreOffset(consumeResult);
                    }
                }
                catch (ConsumeException ex)
                {
                    _logger.LogError(ex, "Erro ao consumir mensagem do Kafka");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao processar mensagem");
                }
            }
        }
        finally
        {
            _consumer.Close();
            _producer.Dispose();
        }
    }

    private async Task ProcessarTransferenciaAsync(
        ConsumeResult<string, string> result,
        CancellationToken cancellationToken)
    {
        var messageValue = result.Message.Value;
        _logger.LogInformation("Processando transferência: {Message}", messageValue);

        try
        {
            var transferencia = JsonSerializer.Deserialize<Program>(
                messageValue,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (transferencia == null)
            {
                _logger.LogWarning("Não foi possível deserializar a mensagem");
                return;
            }

            // Registrar tarifa no banco de dados
            var idTarifacao = await RegistrarTarifaNoBancoAsync(transferencia);

            _logger.LogInformation(
                "Tarifa registrada: ID {IdTarifacao}, Conta {IdConta}, Valor R$ {Valor}",
                idTarifacao,
                transferencia,
                _valorTarifa);

            // Publicar evento de tarifação realizada
            await PublicarTarifacaoRealizadaAsync(idTarifacao, transferencia, cancellationToken);

            _logger.LogInformation("Tarifação processada com sucesso: {IdTarifacao}", idTarifacao);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Erro ao deserializar evento de transferência");
            throw;
        }
    }

    private async Task<string> RegistrarTarifaNoBancoAsync(Program transferencia)
    {
        var idTarifa = Guid.NewGuid().ToString();
        var dataMovimento = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");

        const string sql = @"
            INSERT INTO tarifa (idtarifa, idcontacorrente, datamovimento, valor)
            VALUES (@IdTarifa, @IdContaCorrente, @DataMovimento, @Valor)";

        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        await connection.ExecuteAsync(sql, new
        {
            IdTarifa = idTarifa,
            IdContaCorrente = transferencia,
            DataMovimento = dataMovimento,
            Valor = _valorTarifa
        });

        return idTarifa;
    }

    private async Task PublicarTarifacaoRealizadaAsync(
        string idTarifacao,
        Program transferencia,
        CancellationToken cancellationToken)
    {
        var evento = new TarifacaoRealizadaEvent
        {
            IdTarifacao = idTarifacao,
            IdContaCorrente = transferencia.ToString(),
            ValorTarifado = _valorTarifa,
            DataTarifacao = DateTime.Now
        };

        var eventoJson = JsonSerializer.Serialize(evento);

        var message = new Message<string, string>
        {
            Key = idTarifacao,
            Value = eventoJson,
            Headers = new Headers
            {
                { "event-type", System.Text.Encoding.UTF8.GetBytes("TarifacaoRealizadaEvent") },
                { "timestamp", System.Text.Encoding.UTF8.GetBytes(DateTime.UtcNow.ToString("o")) }
            }
        };

        var deliveryResult = await _producer.ProduceAsync("tarifacoes", message, cancellationToken);

        _logger.LogInformation(
            "Evento de tarifação publicado: Topic {Topic}, Partition {Partition}, Offset {Offset}",
            deliveryResult.Topic,
            deliveryResult.Partition.Value,
            deliveryResult.Offset.Value);
    }

    public override void Dispose()
    {
        _consumer?.Close();
        _consumer?.Dispose();
        _producer?.Dispose();
        base.Dispose();
    }
}