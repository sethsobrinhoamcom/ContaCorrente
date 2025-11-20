namespace ContaCorrente.Domain.Entities;

public sealed class Tarifa
{
    public string IdTarifa { get; set; } = Guid.NewGuid().ToString();
    public string IdContaCorrente { get; set; } = string.Empty;
    public string DataMovimento { get; set; } = string.Empty;
    public decimal Valor { get; set; }
}
