namespace ContaCorrente.Application.DTOs;

public record MovimentoDto
{
    public string IdMovimento { get; init; } = string.Empty;
    public string DataMovimento { get; init; } = string.Empty;
    public char TipoMovimento { get; init; }
    public decimal Valor { get; init; }
}
