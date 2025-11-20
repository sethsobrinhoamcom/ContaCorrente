using System.Text.Json;
using FluentResults;
using MediatR;
using ContaCorrente.Domain.Entities;
using ContaCorrente.Domain.Interfaces;

namespace ContaCorrente.Application.UseCases.Transferencias.Commands.RealizarTransferencia;

public class RealizarTransferenciaCommandHandler : IRequestHandler<RealizarTransferenciaCommand, Result<string>>
{
    private readonly IContaCorrenteRepository _contaRepository;
    private readonly ITransferenciaRepository _transferenciaRepository;
    private readonly IIdempotenciaRepository _idempotenciaRepository;
    private readonly ITarifaRepository _tarifaRepository;

    public RealizarTransferenciaCommandHandler(
        IContaCorrenteRepository contaRepository,
        ITransferenciaRepository transferenciaRepository,
        IIdempotenciaRepository idempotenciaRepository,
        ITarifaRepository tarifaRepository)
    {
        _contaRepository = contaRepository;
        _transferenciaRepository = transferenciaRepository;
        _idempotenciaRepository = idempotenciaRepository;
        _tarifaRepository = tarifaRepository;
    }

    public async Task<Result<string>> Handle(RealizarTransferenciaCommand request, CancellationToken cancellationToken)
    {
        
        if (!string.IsNullOrEmpty(request.ChaveIdempotencia))
        {
            var idempotenciaExistente = await _idempotenciaRepository.ObterAsync(request.ChaveIdempotencia);
            if (idempotenciaExistente != null)
            {
                return Result.Ok(idempotenciaExistente.Resultado);
            }
        }

        var contaOrigem = await _contaRepository.ObterPorIdAsync(request.IdContaCorrenteOrigem);
        if (contaOrigem == null)
        {
            return Result.Fail<string>("Conta de origem não encontrada");
        }

        if (!contaOrigem.Ativo)
        {
            return Result.Fail<string>("Conta de origem está inativa");
        }

        var contaDestino = await _contaRepository.ObterPorIdAsync(request.IdContaCorrenteDestino);
        if (contaDestino == null)
        {
            return Result.Fail<string>("Conta de destino não encontrada");
        }

        if (!contaDestino.Ativo)
        {
            return Result.Fail<string>("Conta de destino está inativa");
        }

       
        var saldoOrigem = await _contaRepository.ObterSaldoAsync(request.IdContaCorrenteOrigem);
        var tarifaValor = 1.00m; 
        var valorTotal = request.Valor + tarifaValor;

        if (saldoOrigem < valorTotal)
        {
            return Result.Fail<string>("Saldo insuficiente");
        }

       
        var dataMovimento = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
        var transferencia = new Transferencia
        {
            IdTransferencia = Guid.NewGuid().ToString(),
            IdContaCorrenteOrigem = request.IdContaCorrenteOrigem,
            IdContaCorrenteDestino = request.IdContaCorrenteDestino,
            DataMovimento = dataMovimento,
            Valor = request.Valor
        };

        await _transferenciaRepository.CriarAsync(transferencia);

        var movimentoDebito = new Movimento
        {
            IdMovimento = Guid.NewGuid().ToString(),
            IdContaCorrente = request.IdContaCorrenteOrigem,
            DataMovimento = dataMovimento,
            TipoMovimento = 'D',
            Valor = request.Valor
        };
        await _contaRepository.CriarMovimentoAsync(movimentoDebito);

        
        var movimentoCredito = new Movimento
        {
            IdMovimento = Guid.NewGuid().ToString(),
            IdContaCorrente = request.IdContaCorrenteDestino,
            DataMovimento = dataMovimento,
            TipoMovimento = 'C',
            Valor = request.Valor
        };
        await _contaRepository.CriarMovimentoAsync(movimentoCredito);

      
        var movimentoTarifa = new Movimento
        {
            IdMovimento = Guid.NewGuid().ToString(),
            IdContaCorrente = request.IdContaCorrenteOrigem,
            DataMovimento = dataMovimento,
            TipoMovimento = 'D',
            Valor = tarifaValor
        };
        await _contaRepository.CriarMovimentoAsync(movimentoTarifa);
       
        var tarifa = new Tarifa
        {
            IdTarifa = Guid.NewGuid().ToString(),
            IdContaCorrente = request.IdContaCorrenteOrigem,
            DataMovimento = dataMovimento,
            Valor = tarifaValor
        };

        await _tarifaRepository.CriarAsync(tarifa);

      
        if (!string.IsNullOrEmpty(request.ChaveIdempotencia))
        {
            var idempotencia = new Idempotencia
            {
                ChaveIdempotencia = request.ChaveIdempotencia,
                Requisicao = JsonSerializer.Serialize(request),
                Resultado = transferencia.IdTransferencia
            };

            await _idempotenciaRepository.SalvarAsync(idempotencia);
        }

        return Result.Ok(transferencia.IdTransferencia);
    }
}