using FluentResults;
using MediatR;

namespace ContaCorrente.Application.UseCases.Auth.Commands.Login;

public record LoginCommand : IRequest<Result<LoginResponse>>
{
    public string CpfOrNumeroConta { get; init; } = string.Empty;
    public string Senha { get; init; } = string.Empty;
}

public record LoginResponse
{
    public string Token { get; init; } = string.Empty;
    public string IdContaCorrente { get; init; } = string.Empty;
    public string NumeroConta { get; init; } = string.Empty;
    public string Nome { get; init; } = string.Empty;
}