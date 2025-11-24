namespace ContaCorrente.Api.Models.Response;

public record RealizarSaqueResponse
{
    public string IdMovimento { get; init; } = string.Empty;
    public string Mensagem { get; init; } = string.Empty;
}
