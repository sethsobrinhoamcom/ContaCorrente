using Dapper;
using ContaCorrente.Domain.Entities;
using ContaCorrente.Domain.Interfaces;
using ContaCorrente.Infrastructure.Data;

namespace ContaCorrente.Infrastructure.Repositories;

public class TransferenciaRepository : ITransferenciaRepository
{
    private readonly IDatabaseContext _context;

    public TransferenciaRepository(IDatabaseContext context)
    {
        _context = context;
    }

    public async Task<string> CriarAsync(Transferencia transferencia)
    {
        const string sql = @"
            INSERT INTO transferencia (idtransferencia, idcontacorrente_origem, idcontacorrente_destino, datamovimento, valor)
            VALUES (@IdTransferencia, @IdContaCorrenteOrigem, @IdContaCorrenteDestino, @DataMovimento, @Valor)";

        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(sql, transferencia);

        return transferencia.IdTransferencia;
    }

    public async Task<Transferencia?> ObterPorIdAsync(string id)
    {
        const string sql = "SELECT * FROM transferencia WHERE idtransferencia = @Id";

        using var connection = _context.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<Transferencia>(sql, new { Id = id });
    }

    public async Task<IEnumerable<Transferencia>> ObterPorContaAsync(string idContaCorrente)
    {
        const string sql = @"
            SELECT * FROM transferencia 
            WHERE idcontacorrente_origem = @Id OR idcontacorrente_destino = @Id";

        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<Transferencia>(sql, new { Id = idContaCorrente });
    }
}