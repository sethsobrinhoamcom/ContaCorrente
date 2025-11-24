using System.Text.Json;
using FluentResults;
using MediatR;
using ContaCorrente.Domain.Entities;
using ContaCorrente.Domain.Events;
using ContaCorrente.Domain.Interfaces;

namespace ContaCorrente.Application.UseCases.ContasCorrentes.Commands.RealizarDeposito;

public class RealizarDepositoCommandHandler : IRequestHandler<RealizarDepositoCommand, Result<string>>
{
    private readonly IContaCorrenteRepository _contaRepository;
    private readonly IIdempotenciaRepository _idempotenciaRepository;
    private readonly IEventPublisher _eventPublisher;

    public RealizarDepositoCommandHandler(
        IContaCorrenteRepository contaRepository,
        IIdempotenciaRepository idempotenciaRepository,
        IEventPublisher eventPublisher)
    {
        _contaRepository = contaRepository;
        _idempotenciaRepository = idempotenciaRepository;
        _eventPublisher = eventPublisher;
    }

    public async Task<Result<string>> Handle(RealizarDepositoCommand request, CancellationToken cancellationToken)
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

        // Criar movimento de crédito
        var dataMovimento = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
        var movimento = new Movimento
        {
            IdMovimento = Guid.NewGuid().ToString(),
            IdContaCorrente = request.IdContaCorrente,
            DataMovimento = dataMovimento,
            TipoMovimento = 'C',
            Valor = request.Valor
        };

        await _contaRepository.CriarMovimentoAsync(movimento);

        // Publicar evento no Kafka
        var evento = new DepositoRealizadoEvent
        {
            IdContaCorrente = request.IdContaCorrente,
            IdMovimento = movimento.IdMovimento,
            Valor = request.Valor,
            DataMovimento = DateTime.Now
        };

        await _eventPublisher.PublishAsync("depositos", movimento.IdMovimento, evento, cancellationToken);

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