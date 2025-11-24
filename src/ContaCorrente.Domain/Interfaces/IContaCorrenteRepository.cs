using ContaCorrente.Domain.Entities;

namespace ContaCorrente.Domain.Interfaces;
public interface IContaCorrenteRepository
{
    Task<ContaCorrenteEntity?> ObterPorIdAsync(string id);
    Task<ContaCorrenteEntity?> ObterPorNumeroAsync(int numero);
    Task<ContaCorrenteEntity?> ObterPorCpfAsync(string cpf);
    Task<string> CriarAsync(ContaCorrenteEntity conta);
    Task<bool> AtualizarAsync(ContaCorrenteEntity conta);
    Task<decimal> ObterSaldoAsync(string idContaCorrente);
    Task<IEnumerable<Movimento>> ObterMovimentosAsync(string idContaCorrente, DateTime? dataInicio = null, DateTime? dataFim = null);
    Task<string> CriarMovimentoAsync(Movimento movimento);
}
