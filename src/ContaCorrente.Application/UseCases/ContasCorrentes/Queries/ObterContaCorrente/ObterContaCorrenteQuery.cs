using FluentResults;
using MediatR;
using ContaCorrente.Application.DTOs;

namespace ContaCorrente.Application.UseCases.ContasCorrentes.Queries.ObterContaCorrente;

public record ObterContaCorrenteQuery : IRequest<Result<ContaCorrenteDto>>
{
    public string IdContaCorrente { get; init; } = string.Empty;
}