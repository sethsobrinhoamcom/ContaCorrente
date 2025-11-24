using FluentResults;
using MediatR;

namespace ContaCorrente.Application.UseCases.ContasCorrentes.Commands.CriarContaCorrente;

public record CriarContaCorrenteCommand : IRequest<Result<string>>
{
    public int Numero { get; init; }
    public string Cpf { get; init; } = string.Empty;
    public string Nome { get; init; } = string.Empty;
    public string Senha { get; init; } = string.Empty;
}