namespace ContaCorrente.Api.Models.Request;

public record RealizarSaqueRequest
{
    public decimal Valor { get; init; }
}