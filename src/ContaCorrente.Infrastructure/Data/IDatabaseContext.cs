using System.Data;

namespace ContaCorrente.Infrastructure.Data;

public interface IDatabaseContext
{
    IDbConnection CreateConnection();
}
