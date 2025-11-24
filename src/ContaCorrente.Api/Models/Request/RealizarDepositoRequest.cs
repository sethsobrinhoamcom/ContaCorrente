namespace ContaCorrente.Api.Models.Request;

public record RealizarDepositoRequest
{
    public decimal Valor { get; init; }
}