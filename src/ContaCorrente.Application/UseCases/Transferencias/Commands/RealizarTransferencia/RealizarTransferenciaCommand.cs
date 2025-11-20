using FluentResults;
using MediatR;

namespace ContaCorrente.Application.UseCases.Transferencias.Commands.RealizarTransferencia;

public record RealizarTransferenciaCommand : IRequest<Result<string>>
{
    public string IdContaCorrenteOrigem { get; init; } = string.Empty;
    public string IdContaCorrenteDestino { get; init; } = string.Empty;
    public decimal Valor { get; init; }
    public string? ChaveIdempotencia { get; init; }
}