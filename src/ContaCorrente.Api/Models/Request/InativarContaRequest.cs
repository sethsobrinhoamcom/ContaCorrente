namespace ContaCorrente.Api.Models.Request;

public record InativarContaRequest
{
    public string Senha { get; init; } = string.Empty;
}