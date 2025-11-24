using System.Text.Json;
using FluentResults;
using MediatR;
using ContaCorrente.Domain.Entities;
using ContaCorrente.Domain.Events;
using ContaCorrente.Domain.Interfaces;

namespace ContaCorrente.Application.UseCases.ContasCorrentes.Commands.RealizarSaque;

public class RealizarSaqueCommandHandler : IRequestHandler<RealizarSaqueCommand, Result<string>>
{
    private readonly IContaCorrenteRepository _contaRepository;
    private readonly IIdempotenciaRepository _idempotenciaRepository;
    private readonly ITarifaRepository _tarifaRepository;
    private readonly IEventPublisher _eventPublisher;

    public RealizarSaqueCommandHandler(
        IContaCorrenteRepository contaRepository,
        IIdempotenciaRepository idempotenciaRepository,
        ITarifaRepository tarifaRepository,
        IEventPublisher eventPublisher)
    {
        _contaRepository = contaRepository;
        _idempotenciaRepository = idempotenciaRepository;
        _tarifaRepository = tarifaRepository;
        _eventPublisher = eventPublisher;
    }

    public async Task<Result<string>> Handle(RealizarSaqueCommand request, CancellationToken cancellationToken)
    {
        // Verificar idempotência
        if (!string.IsNullOrEmpty(request.ChaveIdempotencia))
        {
            var idempotenciaExistente = await _idempotenciaRepository.ObterAsync(request.ChaveIdempotencia);
            if (idempotenciaExistente != null)
            {
                return Result.Ok(idempotenciaExistente.Resultado);
            }
        }

        // Validar conta
        var conta = await _contaRepository.ObterPorIdAsync(request.IdContaCorrente);
        if (conta == null)
        {
            return Result.Fail<string>("Conta corrente não encontrada");
        }

        if (!conta.Ativo)
        {
            return Result.Fail<string>("Conta corrente está inativa");
        }

        // Verificar saldo
        var saldo = await _contaRepository.ObterSaldoAsync(request.IdContaCorrente);
        var tarifaValor = 0.50m; // Tarifa de R$ 0,50 por saque
        var valorTotal = request.Valor + tarifaValor;

        if (saldo < valorTotal)
        {
            return Result.Fail<string>($"Saldo insuficiente. Saldo disponível: R$ {saldo:F2}");
        }

        var dataMovimento = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");

        // Criar movimento de débito
        var movimento = new Movimento
        {
            IdMovimento = Guid.NewGuid().ToString(),
            IdContaCorrente = request.IdContaCorrente,
            DataMovimento = dataMovimento,
            TipoMovimento = 'D',
            Valor = request.Valor
        };

        await _contaRepository.CriarMovimentoAsync(movimento);

        // Criar tarifa
        var tarifa = new Tarifa
        {
            IdTarifa = Guid.NewGuid().ToString(),
            IdContaCorrente = request.IdContaCorrente,
            DataMovimento = dataMovimento,
            Valor = tarifaValor
        };

        await _tarifaRepository.CriarAsync(tarifa);

        // Criar movimento da tarifa
        var movimentoTarifa = new Movimento
        {
            IdMovimento = Guid.NewGuid().ToString(),
            IdContaCorrente = request.IdContaCorrente,
            DataMovimento = dataMovimento,
            TipoMovimento = 'D',
            Valor = tarifaValor
        };

        await _contaRepository.CriarMovimentoAsync(movimentoTarifa);

        // Publicar evento no Kafka
        var evento = new SaqueRealizadoEvent
        {
            IdContaCorrente = request.IdContaCorrente,
            IdMovimento = movimento.IdMovimento,
            Valor = request.Valor,
            DataMovimento = DateTime.Now
        };

        await _eventPublisher.PublishAsync("saques", movimento.IdMovimento, evento, cancellationToken);

        // Salvar idempotência
        if (!string.IsNullOrEmpty(request.ChaveIdempotencia))
        {
            var idempotencia = new Idempotencia
            {
                ChaveIdempotencia = request.ChaveIdempotencia,
                Requisicao = JsonSerializer.Serialize(request),
                Resultado = movimento.IdMovimento
            };

            await _idempotenciaRepository.SalvarAsync(idempotencia);
        }

        return Result.Ok(movimento.IdMovimento);
    }
}