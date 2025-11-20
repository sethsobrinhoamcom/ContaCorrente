namespace ContaCorrente.Api.Models.Request;

public record RealizarTransferenciaRequest
{
    public string IdContaCorrenteOrigem { get; init; } = string.Empty;
    public string IdContaCorrenteDestino { get; init; } = string.Empty;
    public decimal Valor { get; init; }
}