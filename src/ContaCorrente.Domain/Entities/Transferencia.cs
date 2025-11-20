namespace ContaCorrente.Domain.Entities;

public sealed class Transferencia
{
    public string IdTransferencia { get; set; } = Guid.NewGuid().ToString();
    public string IdContaCorrenteOrigem { get; set; } = string.Empty;
    public string IdContaCorrenteDestino { get; set; } = string.Empty;
    public string DataMovimento { get; set; } = string.Empty;
    public decimal Valor { get; set; }
}
