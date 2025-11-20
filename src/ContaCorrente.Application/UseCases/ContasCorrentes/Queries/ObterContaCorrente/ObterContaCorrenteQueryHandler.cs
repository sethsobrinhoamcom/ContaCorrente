using FluentResults;
using MediatR;
using ContaCorrente.Application.DTOs;
using ContaCorrente.Domain.Interfaces;

namespace ContaCorrente.Application.UseCases.ContasCorrentes.Queries.ObterContaCorrente;

public class ObterContaCorrenteQueryHandler : IRequestHandler<ObterContaCorrenteQuery, Result<ContaCorrenteDto>>
{
    private readonly IContaCorrenteRepository _repository;

    public ObterContaCorrenteQueryHandler(IContaCorrenteRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<ContaCorrenteDto>> Handle(ObterContaCorrenteQuery request, CancellationToken cancellationToken)
    {
        var conta = await _repository.ObterPorIdAsync(request.IdContaCorrente);

        if (conta == null)
        {
            return Result.Fail<ContaCorrenteDto>("Conta corrente não encontrada");
        }

        var saldo = await _repository.ObterSaldoAsync(request.IdContaCorrente);

        var dto = new ContaCorrenteDto
        {
            IdContaCorrente = conta.IdContaCorrente,
            Numero = conta.Numero,
            Nome = conta.Nome,
            Ativo = conta.Ativo,
            Saldo = saldo
        };

        return Result.Ok(dto);
    }
}