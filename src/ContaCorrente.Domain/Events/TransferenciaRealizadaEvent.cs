namespace ContaCorrente.Domain.Events;

public record TransferenciaRealizadaEvent
{
    public string IdTransferencia { get; init; } = string.Empty;
    public string IdContaCorrenteOrigem { get; init; } = string.Empty;
    public string IdContaCorrenteDestino { get; init; } = string.Empty;
    public decimal Valor { get; init; }
    public decimal Tarifa { get; init; }
    public DateTime DataMovimento { get; init; }
}