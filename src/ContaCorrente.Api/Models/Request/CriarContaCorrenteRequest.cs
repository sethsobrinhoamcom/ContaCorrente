namespace ContaCorrente.Api.Models.Request;

public record CriarContaCorrenteRequest
{
    public int Numero { get; init; }
    public string Cpf { get; init; } = string.Empty;
    public string Nome { get; init; } = string.Empty;
    public string Senha { get; init; } = string.Empty;
}
