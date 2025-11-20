namespace ContaCorrente.Domain.Entities;

public sealed class Movimento
{
    public string IdMovimento { get; set; } = Guid.NewGuid().ToString();
    public string IdContaCorrente { get; set; } = string.Empty;
    public string DataMovimento { get; set; } = string.Empty;
    public char TipoMovimento { get; set; } // C = Crédito, D = Débito
    public decimal Valor { get; set; }
}
