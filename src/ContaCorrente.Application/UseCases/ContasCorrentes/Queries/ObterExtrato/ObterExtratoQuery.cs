using FluentResults;
using MediatR;
using ContaCorrente.Application.DTOs;

namespace ContaCorrente.Application.UseCases.ContasCorrentes.Queries.ObterExtrato;

public record ObterExtratoQuery : IRequest<Result<IEnumerable<MovimentoDto>>>
{
    public string IdContaCorrente { get; init; } = string.Empty;
    public DateTime? DataInicio { get; init; }
    public DateTime? DataFim { get; init; }
}