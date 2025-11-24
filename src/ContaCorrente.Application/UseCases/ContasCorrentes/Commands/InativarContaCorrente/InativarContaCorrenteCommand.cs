using FluentResults;
using MediatR;

namespace ContaCorrente.Application.UseCases.ContasCorrentes.Commands.InativarContaCorrente;

public record InativarContaCorrenteCommand : IRequest<Result>
{
    public string IdContaCorrente { get; init; } = string.Empty;
    public string Senha { get; init; } = string.Empty;
}