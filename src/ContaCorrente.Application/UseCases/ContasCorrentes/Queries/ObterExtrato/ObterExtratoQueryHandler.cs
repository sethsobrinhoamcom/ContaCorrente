using FluentResults;
using MediatR;
using ContaCorrente.Application.DTOs;
using ContaCorrente.Domain.Interfaces;

namespace ContaCorrente.Application.UseCases.ContasCorrentes.Queries.ObterExtrato;

public class ObterExtratoQueryHandler : IRequestHandler<ObterExtratoQuery, Result<IEnumerable<MovimentoDto>>>
{
    private readonly IContaCorrenteRepository _repository;

    public ObterExtratoQueryHandler(IContaCorrenteRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IEnumerable<MovimentoDto>>> Handle(ObterExtratoQuery request, CancellationToken cancellationToken)
    {
        var conta = await _repository.ObterPorIdAsync(request.IdContaCorrente);

        if (conta == null)
        {
            return Result.Fail<IEnumerable<MovimentoDto>>("Conta corrente não encontrada");
        }

        var movimentos = await _repository.ObterMovimentosAsync(
            request.IdContaCorrente,
            request.DataInicio,
            request.DataFim);

        var dtos = movimentos.Select(m => new MovimentoDto
        {
            IdMovimento = m.IdMovimento,
            DataMovimento = m.DataMovimento,
            TipoMovimento = m.TipoMovimento,
            Valor = m.Valor
        });

        return Result.Ok(dtos);
    }
}