using Dapper;
using ContaCorrente.Infrastructure.Data;

namespace ContaCorrente.Infrastructure.Data;

public class DatabaseInitializer
{
    private readonly IDatabaseContext _context;

    public DatabaseInitializer(IDatabaseContext context)
    {
        _context = context;
    }

    public async Task InitializeAsync()
    {
        using var connection = _context.CreateConnection();

        await connection.ExecuteAsync(@"
            CREATE TABLE IF NOT EXISTS contacorrente (
                idcontacorrente TEXT(37) PRIMARY KEY,
                numero INTEGER(10) NOT NULL UNIQUE,
                cpf TEXT(11) NOT NULL UNIQUE,
                nome TEXT(100) NOT NULL,
                ativo INTEGER(1) NOT NULL default 1,
                senha TEXT(100) NOT NULL,
                salt TEXT(100) NOT NULL,
                CHECK (ativo in (0,1))
            )");

        await connection.ExecuteAsync(@"
            CREATE TABLE IF NOT EXISTS movimento (
                idmovimento TEXT(37) PRIMARY KEY,
                idcontacorrente TEXT(37) NOT NULL,
                datamovimento TEXT(25) NOT NULL,
                tipomovimento TEXT(1) NOT NULL,
                valor REAL NOT NULL,
                CHECK (tipomovimento in ('C','D')),
                FOREIGN KEY(idcontacorrente) REFERENCES contacorrente(idcontacorrente)
            )");

     
        await connection.ExecuteAsync(@"
            CREATE TABLE IF NOT EXISTS transferencia (
                idtransferencia TEXT(37) PRIMARY KEY,
                idcontacorrente_origem TEXT(37) NOT NULL,
                idcontacorrente_destino TEXT(37) NOT NULL,
                datamovimento TEXT(25) NOT NULL,
                valor REAL NOT NULL,
                FOREIGN KEY(idcontacorrente_origem) REFERENCES contacorrente(idcontacorrente),
                FOREIGN KEY(idcontacorrente_destino) REFERENCES contacorrente(idcontacorrente)
            )");

        await connection.ExecuteAsync(@"
            CREATE TABLE IF NOT EXISTS tarifa (
                idtarifa TEXT(37) PRIMARY KEY,
                idcontacorrente TEXT(37) NOT NULL,
                datamovimento TEXT(25) NOT NULL,
                valor REAL NOT NULL,
                FOREIGN KEY(idcontacorrente) REFERENCES contacorrente(idcontacorrente)
            )");

       
        await connection.ExecuteAsync(@"
            CREATE TABLE IF NOT EXISTS idempotencia (
                chave_idempotencia TEXT(37) PRIMARY KEY,
                requisicao TEXT(1000),
                resultado TEXT(1000)
            )");
    }
}