using Confluent.Kafka;
using ContaCorrente.Domain.Entities;
using ContaCorrente.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace ContaCorrente.Infrastructure.Messaging;

public class TarifacaoConsumerService : BackgroundService
{
    private readonly IConsumer<string, string> _consumer;
    private readonly ILogger<TarifacaoConsumerService> _logger;
    private readonly IServiceProvider _serviceProvider;

    public TarifacaoConsumerService(
        IConfiguration configuration,
        ILogger<TarifacaoConsumerService> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;

        var config = new ConsumerConfig
        {
            BootstrapServers = configuration["Kafka:BootstrapServers"] ?? "localhost:9092",
            GroupId = "tarifacoes-api-consumer-group",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false
        };

        _consumer = new ConsumerBuilder<string, string>(config).Build();
        _logger.LogInformation("TarifacaoConsumerService inicializado");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _consumer.Subscribe("tarifacoes");
        _logger.LogInformation("Consumer inscrito no tópico 'tarifacoes'");

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var consumeResult = _consumer.Consume(stoppingToken);

                    if (consumeResult?.Message != null)
                    {
                        await ProcessarTarifacaoAsync(consumeResult, stoppingToken);

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
                    _logger.LogError(ex, "Erro ao processar tarifação");
                }
            }
        }
        finally
        {
            _consumer.Close();
        }
    }

    private async Task ProcessarTarifacaoAsync(
        ConsumeResult<string, string> result,
        CancellationToken cancellationToken)
    {
        var messageValue = result.Message.Value;
        _logger.LogInformation("Processando tarifação: {Message}", messageValue);

        try
        {
            var tarifacao = JsonSerializer.Deserialize<TarifacaoRealizadaEventDto>(
                messageValue,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (tarifacao == null)
            {
                _logger.LogWarning("Não foi possível deserializar a tarifação");
                return;
            }

            // Criar movimento de débito para a tarifa
            using var scope = _serviceProvider.CreateScope();
            var contaRepository = scope.ServiceProvider.GetRequiredService<IContaCorrenteRepository>();

            var dataMovimento = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
            var movimento = new Movimento
            {
                IdMovimento = Guid.NewGuid().ToString(),
                IdContaCorrente = tarifacao.IdContaCorrente,
                DataMovimento = dataMovimento,
                TipoMovimento = 'D',
                Valor = tarifacao.ValorTarifado
            };

            await contaRepository.CriarMovimentoAsync(movimento);

            _logger.LogInformation(
                "Débito de tarifa registrado: Conta {IdConta}, Valor R$ {Valor}",
                tarifacao.IdContaCorrente,
                tarifacao.ValorTarifado);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Erro ao deserializar evento de tarifação");
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

internal record TarifacaoRealizadaEventDto
{
    public string IdTarifacao { get; init; } = string.Empty;
    public string IdContaCorrente { get; init; } = string.Empty;
    public decimal ValorTarifado { get; init; }
    public DateTime DataTarifacao { get; init; }
}