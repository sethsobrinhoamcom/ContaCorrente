using ContaCorrente.Domain.Entities;

namespace ContaCorrente.Domain.Interfaces;
public interface ITarifaRepository
{
    Task<string> CriarAsync(Tarifa tarifa);
    Task<IEnumerable<Tarifa>> ObterPorContaAsync(string idContaCorrente);
}
