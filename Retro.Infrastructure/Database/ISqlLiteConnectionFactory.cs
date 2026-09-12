using Microsoft.Data.Sqlite;

namespace Retro.Infrastructure.Database;

public interface ISqliteConnectionFactory
{
    SqliteConnection CreateConnection();
}