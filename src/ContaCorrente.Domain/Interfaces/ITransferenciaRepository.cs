using ContaCorrente.Domain.Entities;

namespace ContaCorrente.Domain.Interfaces;

public interface ITransferenciaRepository
{
    Task<string> CriarAsync(Transferencia transferencia);
    Task<Transferencia?> ObterPorIdAsync(string id);
    Task<IEnumerable<Transferencia>> ObterPorContaAsync(string idContaCorrente);
}
