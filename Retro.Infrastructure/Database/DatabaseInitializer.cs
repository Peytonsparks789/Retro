using Microsoft.Data.Sqlite;
using Retro.Core.Enums;

namespace Retro.Infrastructure.Database;

public sealed class DatabaseInitializer(ISqliteConnectionFactory connectionFactory)
{
    public void Initialize()
    {
        using var connection = connectionFactory.CreateConnection();
        connection.Open();

        var mediumRecordInserts = string.Join(
            "\n",
            Enum.GetValues<MediumType>()
                .Select(m =>
                    $"INSERT INTO Mediums (Id, Name) VALUES ({(int)m}, '{m}')  ON CONFLICT(Id) DO UPDATE SET Name = '{m}';")
        );

        var mediaInit = Queries.MediaDbTablesInit(mediumRecordInserts);

        using var mediaInitCommand = new SqliteCommand(mediaInit, connection);
        mediaInitCommand.ExecuteNonQuery();
    }
}