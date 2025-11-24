namespace ContaCorrente.Api.Models.Request;

public record RealizarDepositoRequest
{
    public string? IdContaCorrente { get; init; }
    public decimal Valor { get; init; }
}