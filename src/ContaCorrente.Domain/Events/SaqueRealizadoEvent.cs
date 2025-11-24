namespace ContaCorrente.Domain.Events;

public record SaqueRealizadoEvent
{
    public string IdContaCorrente { get; init; } = string.Empty;
    public string IdMovimento { get; init; } = string.Empty;
    public decimal Valor { get; init; }
    public DateTime DataMovimento { get; init; }
}