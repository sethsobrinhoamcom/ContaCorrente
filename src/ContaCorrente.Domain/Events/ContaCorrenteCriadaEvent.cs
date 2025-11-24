namespace ContaCorrente.Domain.Events;

public record ContaCorrenteCriadaEvent
{
    public string IdContaCorrente { get; init; } = string.Empty;
    public int Numero { get; init; }
    public string Nome { get; init; } = string.Empty;
    public DateTime DataCriacao { get; init; }
}