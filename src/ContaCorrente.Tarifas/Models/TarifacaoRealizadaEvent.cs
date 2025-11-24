namespace ContaCorrente.Tarifas.Models;

public record TarifacaoRealizadaEvent
{
    public string IdTarifacao { get; init; } = string.Empty;
    public string IdContaCorrente { get; init; } = string.Empty;
    public decimal ValorTarifado { get; init; }
    public DateTime DataTarifacao { get; init; }
}