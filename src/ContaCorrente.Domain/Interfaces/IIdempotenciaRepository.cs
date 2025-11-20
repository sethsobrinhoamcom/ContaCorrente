using ContaCorrente.Domain.Entities;

namespace ContaCorrente.Domain.Interfaces;

public interface IIdempotenciaRepository
{
    Task<Idempotencia?> ObterAsync(string chave);
    Task SalvarAsync(Idempotencia idempotencia);
}
