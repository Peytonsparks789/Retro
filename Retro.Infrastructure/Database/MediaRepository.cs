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
        using var connection = _sqliteConnectionFactory.CreateConnection();
        connection.Open();

        var mediumRecordInserts = string.Join(
            "\n",
            Enum.GetValues<MediumType>()
                .Select(m => $"INSERT INTO Mediums (Id, Name) VALUES ({(int)m}, '{m}')  ON CONFLICT(Id) DO UPDATE SET Name = '{m}';")
        );

        var mediaInit = Queries.MediaDbTablesInit(mediumRecordInserts);
        
        using var mediaInitCommand = new SqliteCommand(mediaInit, connection);
        mediaInitCommand.ExecuteNonQuery();
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
