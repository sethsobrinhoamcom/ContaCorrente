

namespace ContaCorrente.Application.DTOs;

public record ContaCorrenteDto
{
    public string IdContaCorrente { get; init; } = string.Empty;
    public int Numero { get; init; }
    public string Nome { get; init; } = string.Empty;
    public bool Ativo { get; init; }
    public decimal Saldo { get; init; }
}
