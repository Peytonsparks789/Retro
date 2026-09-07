using Microsoft.Extensions.DependencyInjection;

namespace Retro.Infrastructure.Database.DependencyInjection;

public static class MediaRepositoryExtensions
{
    public static IServiceCollection AddMediaRepository(
        this IServiceCollection services)
    {
        services.AddSingleton<IMediaRepository, MediaRepository>();
        
        return services;
    }
}