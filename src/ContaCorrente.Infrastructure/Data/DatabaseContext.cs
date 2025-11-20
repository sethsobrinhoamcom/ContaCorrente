

using Microsoft.Data.Sqlite;
using System.Data;

namespace ContaCorrente.Infrastructure.Data;

public class DatabaseContext : IDatabaseContext
{
    private readonly string _connectionString;

    public DatabaseContext(string connectionString)
    {
        _connectionString = connectionString;
    }

    public IDbConnection CreateConnection()
    {
        return new SqliteConnection(_connectionString);
    }
}
