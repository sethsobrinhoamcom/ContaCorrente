using FluentResults;
using MediatR;

namespace ContaCorrente.Application.UseCases.ContasCorrentes.Commands.RealizarSaque;

public record RealizarSaqueCommand : IRequest<Result<string>>
{
    public string IdContaCorrente { get; init; } = string.Empty;
    public decimal Valor { get; init; }
    public string? ChaveIdempotencia { get; init; }
}