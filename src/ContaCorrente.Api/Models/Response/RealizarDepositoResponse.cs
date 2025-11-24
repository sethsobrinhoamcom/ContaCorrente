namespace ContaCorrente.Api.Models.Response;

public record RealizarDepositoResponse
{
    public string IdMovimento { get; init; } = string.Empty;
    public string Mensagem { get; init; } = string.Empty;
}
