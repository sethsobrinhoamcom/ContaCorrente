namespace ContaCorrente.Tarifas.Models;

public record Program
{
    public string IdTransferencia { get; init; } = string.Empty;
    public string IdContaCorrenteOrigem { get; init; } = string.Empty;
    public string IdRequisicao { get; init; } = string.Empty;
    public decimal ValorTransferencia { get; init; }
    public DateTime DataMovimento { get; init; }
}