using Microsoft.Extensions.DependencyInjection;

namespace Retro.Infrastructure.Database.DependencyInjection;

public static class SqliteDbExtensions
{
    public static IServiceCollection AddSqliteDb(
        this IServiceCollection services)
    {
        services.AddSingleton<ISqliteConnectionFactory, SqliteConnectionFactory>(
            _ => new SqliteConnectionFactory());
        
        services.AddMediaRepository();
        
        return services;
    }
}