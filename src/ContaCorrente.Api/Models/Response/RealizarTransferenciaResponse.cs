namespace ContaCorrente.Api.Models.Response;

public record RealizarTransferenciaResponse
{
    public string IdTransferencia { get; init; } = string.Empty;
    public string Mensagem { get; init; } = string.Empty;
}
