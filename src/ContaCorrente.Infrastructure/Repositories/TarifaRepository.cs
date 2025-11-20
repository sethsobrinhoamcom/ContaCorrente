using Dapper;
using ContaCorrente.Domain.Entities;
using ContaCorrente.Domain.Interfaces;
using ContaCorrente.Infrastructure.Data;

namespace ContaCorrente.Infrastructure.Repositories;

public class TarifaRepository : ITarifaRepository
{
    private readonly IDatabaseContext _context;

    public TarifaRepository(IDatabaseContext context)
    {
        _context = context;
    }

    public async Task<string> CriarAsync(Tarifa tarifa)
    {
        const string sql = @"
            INSERT INTO tarifa (idtarifa, idcontacorrente, datamovimento, valor)
            VALUES (@IdTarifa, @IdContaCorrente, @DataMovimento, @Valor)";

        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(sql, tarifa);

        return tarifa.IdTarifa;
    }

    public async Task<IEnumerable<Tarifa>> ObterPorContaAsync(string idContaCorrente)
    {
        const string sql = "SELECT * FROM tarifa WHERE idcontacorrente = @IdContaCorrente";

        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<Tarifa>(sql, new { IdContaCorrente = idContaCorrente });
    }
}