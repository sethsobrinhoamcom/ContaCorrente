namespace ContaCorrente.Application.DTOs;

public record TransferenciaDto
{
    public string IdTransferencia { get; init; } = string.Empty;
    public string IdContaCorrenteOrigem { get; init; } = string.Empty;
    public string IdContaCorrenteDestino { get; init; } = string.Empty;
    public string DataMovimento { get; init; } = string.Empty;
    public decimal Valor { get; init; }
}