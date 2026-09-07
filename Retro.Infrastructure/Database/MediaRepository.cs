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

    public async Task UpsertMediaAsync(Media media)
    {
        ArgumentNullException.ThrowIfNull(media);
        
        await using var connection = _sqliteConnectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var transaction = connection.BeginTransaction();

        try
        {
            // Upsert SubMediums table
            long? subMediumId = null;
            if (media.SubMedium != null)
            {
                await using var command = connection.CreateCommand();
                command.Transaction = transaction;
                command.CommandText = Queries.UpsertSubMediumEntry();
                command.Parameters.AddWithValue("name", media.SubMedium);
                subMediumId = (long?)command.ExecuteScalar();
            }
            
            // Upsert Media table
            long? mediaId;
            await using (var command = connection.CreateCommand())
            {
                command.Transaction = transaction;
                switch (media.Id)
                {
                    case null:
                        command.CommandText = Queries.InsertMediaEntry();
                        break;

                    case not null:
                        command.CommandText = Queries.UpsertMediaEntry();
                        command.Parameters.AddWithValue("id", media.Id);
                        break;
                }
                command.Parameters.AddWithValue("title", media.Title);
                command.Parameters.AddWithValue("mediumId", media.Medium);
                command.Parameters.AddWithValue("subMediumId", (object?)subMediumId ?? DBNull.Value);
                command.Parameters.AddWithValue("description", (object?)media.Description ?? DBNull.Value);
                mediaId = (long?)command.ExecuteScalar();
                ArgumentNullException.ThrowIfNull(mediaId);
            }
            
            foreach (var genre in media.Genres)
            {
                // Upsert Genres table
                long? genreId;
                await using (var command = connection.CreateCommand())
                {
                    command.Transaction = transaction;
                    command.CommandText = Queries.UpsertGenreEntry();
                    command.Parameters.AddWithValue("$name", genre);
                    genreId = (long?)command.ExecuteScalar();
                    ArgumentNullException.ThrowIfNull(genreId);
                }

                // Upsert MediaGenres table
                await using (var command = connection.CreateCommand())
                {
                    command.Transaction = transaction;
                    command.CommandText = Queries.UpsertMediaGenresEntry();
                    command.Parameters.AddWithValue("mediaId", mediaId);
                    command.Parameters.AddWithValue("genreId", genreId);
                    command.ExecuteNonQuery();
                }
            }
            
            // Upsert MediaReviews table
            long? reviewId;
            await using (var command = connection.CreateCommand())
            {
                command.Transaction = transaction;
                command.CommandText = Queries.UpsertMediaReviewEntry();
                command.Parameters.AddWithValue("mediaId", mediaId);
                command.Parameters.AddWithValue("stars", media.Review.Stars);
                command.Parameters.AddWithValue("thoughts", media.Review.Thoughts);
                reviewId = (long?)command.ExecuteScalar();
                ArgumentNullException.ThrowIfNull(reviewId);
            }
            
            // Upsert MediaReviewPros table
            foreach (var pro in media.Review.Pros)
            {
                await using var command = connection.CreateCommand();
                command.Transaction = transaction;
                command.CommandText = Queries.UpsertMediaReviewProsEntry();
                command.Parameters.AddWithValue("mediaReviewId", reviewId);
                command.Parameters.AddWithValue("description", pro);
                command.ExecuteNonQuery();
            }
            
            // Upsert MediaReviewCons table
            foreach (var con in media.Review.Cons)
            {
                await using var command = connection.CreateCommand();
                command.Transaction = transaction;
                command.CommandText = Queries.UpsertMediaReviewConsEntry();
                command.Parameters.AddWithValue("mediaReviewId", reviewId);
                command.Parameters.AddWithValue("description", con);
                command.ExecuteNonQuery();
            }
            
            // Upsert MediaConsumptionHistory table
            foreach (var consumedAt in media.ConsumptionHistory)
            {
                await using var command = connection.CreateCommand();
                command.Transaction = transaction;
                command.CommandText = Queries.UpsertMediaConsumptionHistoryEntry();
                command.Parameters.AddWithValue("mediaId", mediaId);
                command.Parameters.AddWithValue("consumedAt", consumedAt);
                command.ExecuteNonQuery();
            }
            transaction.Commit();
        }
        catch (Exception)
        {
            transaction.Rollback();
            throw;
        }
    }
    
    public Task<Media> ReadMediaAsync(string id)
    {
        throw new NotImplementedException();
    }
    
    public Task DeleteMediaAsync(string id)
    {
        throw new NotImplementedException();
    }
}
