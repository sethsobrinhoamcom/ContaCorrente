using Dapper;
using ContaCorrente.Domain.Entities;
using ContaCorrente.Domain.Interfaces;
using ContaCorrente.Infrastructure.Data;

namespace ContaCorrente.Infrastructure.Repositories;

public class ContaCorrenteRepository : IContaCorrenteRepository
{
    private readonly IDatabaseContext _context;

    public ContaCorrenteRepository(IDatabaseContext context)
    {
        _context = context;
    }

    public async Task<ContaCorrenteEntity?> ObterPorIdAsync(string id)
    {
        const string sql = "SELECT * FROM contacorrente WHERE idcontacorrente = @Id";

        using var connection = _context.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<ContaCorrenteEntity>(sql, new { Id = id });
    }

    public async Task<ContaCorrenteEntity?> ObterPorNumeroAsync(int numero)
    {
        const string sql = "SELECT * FROM contacorrente WHERE numero = @Numero";

        using var connection = _context.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<ContaCorrenteEntity>(sql, new { Numero = numero });
    }

    public async Task<string> CriarAsync(ContaCorrenteEntity conta)
    {
        const string sql = @"
            INSERT INTO contacorrente (idcontacorrente, numero, nome, ativo, senha, salt)
            VALUES (@IdContaCorrente, @Numero, @Nome, @Ativo, @Senha, @Salt)";

        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(sql, new
        {
            conta.IdContaCorrente,
            conta.Numero,
            conta.Nome,
            Ativo = conta.Ativo ? 1 : 0,
            conta.Senha,
            conta.Salt
        });

        return conta.IdContaCorrente;
    }

    public async Task<bool> AtualizarAsync(ContaCorrenteEntity conta)
    {
        const string sql = @"
            UPDATE contacorrente 
            SET nome = @Nome, ativo = @Ativo, senha = @Senha, salt = @Salt
            WHERE idcontacorrente = @IdContaCorrente";

        using var connection = _context.CreateConnection();
        var rows = await connection.ExecuteAsync(sql, new
        {
            conta.IdContaCorrente,
            conta.Nome,
            Ativo = conta.Ativo ? 1 : 0,
            conta.Senha,
            conta.Salt
        });

        return rows > 0;
    }

    public async Task<decimal> ObterSaldoAsync(string idContaCorrente)
    {
        const string sql = @"
            SELECT 
                COALESCE(SUM(CASE WHEN tipomovimento = 'C' THEN valor ELSE -valor END), 0) as Saldo
            FROM movimento
            WHERE idcontacorrente = @IdContaCorrente";

        using var connection = _context.CreateConnection();
        return await connection.QuerySingleAsync<decimal>(sql, new { IdContaCorrente = idContaCorrente });
    }

    public async Task<IEnumerable<Movimento>> ObterMovimentosAsync(string idContaCorrente, DateTime? dataInicio = null, DateTime? dataFim = null)
    {
        var sql = "SELECT * FROM movimento WHERE idcontacorrente = @IdContaCorrente";        

        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<Movimento>(sql, new { IdContaCorrente = idContaCorrente });
    }
    public async Task<string> CriarMovimentoAsync(Movimento movimento)
    {
        const string sql = @"
        INSERT INTO movimento (idmovimento, idcontacorrente, datamovimento, tipomovimento, valor)
        VALUES (@IdMovimento, @IdContaCorrente, @DataMovimento, @TipoMovimento, @Valor)";

        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(sql, movimento);

        return movimento.IdMovimento;
    }
}