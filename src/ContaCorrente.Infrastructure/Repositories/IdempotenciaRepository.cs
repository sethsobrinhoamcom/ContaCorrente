using Dapper;
using ContaCorrente.Domain.Entities;
using ContaCorrente.Domain.Interfaces;
using ContaCorrente.Infrastructure.Data;

namespace ContaCorrente.Infrastructure.Repositories;

public class IdempotenciaRepository : IIdempotenciaRepository
{
    private readonly IDatabaseContext _context;

    public IdempotenciaRepository(IDatabaseContext context)
    {
        _context = context;
    }

    public async Task<Idempotencia?> ObterAsync(string chave)
    {
        const string sql = "SELECT * FROM idempotencia WHERE chave_idempotencia = @Chave";

        using var connection = _context.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<Idempotencia>(sql, new { Chave = chave });
    }

    public async Task SalvarAsync(Idempotencia idempotencia)
    {
        const string sql = @"
            INSERT INTO idempotencia (chave_idempotencia, requisicao, resultado)
            VALUES (@ChaveIdempotencia, @Requisicao, @Resultado)";

        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(sql, idempotencia);
    }
}