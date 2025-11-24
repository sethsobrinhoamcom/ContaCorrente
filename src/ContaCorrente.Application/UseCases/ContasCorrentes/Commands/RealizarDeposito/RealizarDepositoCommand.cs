using FluentResults;
using MediatR;

namespace ContaCorrente.Application.UseCases.ContasCorrentes.Commands.RealizarDeposito;

public record RealizarDepositoCommand : IRequest<Result<string>>
{
    public string IdContaCorrente { get; init; } = string.Empty;
    public decimal Valor { get; init; }
    public string? ChaveIdempotencia { get; init; }
}