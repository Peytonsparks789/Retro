using Microsoft.Data.Sqlite;
using Retro.Core.Data;
using Retro.Core.Enums;

namespace Retro.Infrastructure.Database;

public class MediaRepository : IMediaRepository
{
    private readonly ISqliteConnectionFactory _sqliteConnectionFactory;
    public MediaRepository(ISqliteConnectionFactory connectionFactory)
    {
        _sqliteConnectionFactory = connectionFactory;
        Initialize();
    }
    private void Initialize()
    {
        throw new NotImplementedException();
    }

    public Task CreateMediaAsync(Media media)
    {
        throw new NotImplementedException();
    }
    
    public Task<Media> ReadMediaAsync(string id)
    {
        throw new NotImplementedException();
    }
    
    public Task UpdateMediaAsync(Media media)
    {
        throw new NotImplementedException();
    }
    
    public Task DeleteMediaAsync(string id)
    {
        throw new NotImplementedException();
    }
}
