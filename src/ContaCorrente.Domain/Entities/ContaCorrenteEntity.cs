namespace ContaCorrente.Domain.Entities;

public class ContaCorrenteEntity
{
    public string IdContaCorrente { get; set; } = Guid.NewGuid().ToString();
    public int Numero { get; set; }
    public string Cpf { get; set; } = string.Empty; 
    public string Nome { get; set; } = string.Empty;
    public bool Ativo { get; set; }
    public string Senha { get; set; } = string.Empty;
    public string Salt { get; set; } = string.Empty;

    public List<Movimento> Movimentos { get; set; } = new();
}