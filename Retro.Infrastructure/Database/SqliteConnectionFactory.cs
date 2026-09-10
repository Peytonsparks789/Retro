using Microsoft.Data.Sqlite;

namespace Retro.Infrastructure.Database;

public class SqliteConnectionFactory : ISqliteConnectionFactory
{
    public SqliteConnection CreateConnection()
    {
        var path = Path.Combine(
            AppContext.BaseDirectory,
            "Data",
            "retro.db");
        
        var directory = Path.GetDirectoryName(path);

        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }
        
        return new SqliteConnection($"Data Source={path};Foreign Keys=True");
    }
}