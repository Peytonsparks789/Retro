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
                    if (genre.Id is null)
                    {
                        command.CommandText = Queries.InsertGenreEntry();
                    }
                    else
                    {
                        command.CommandText = Queries.UpdateGenreEntry();
                        command.Parameters.AddWithValue("id", genre.Id);
                    }
                    command.Parameters.AddWithValue("$name", genre.Name);
                    genreId = (long?)command.ExecuteScalar();
                }
                ArgumentNullException.ThrowIfNull(genreId);
                
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
    
    public async Task<List<MediaSummary>> GetMediaSummaryAsync(
        string search,
        int pageSize,
        int lastId)
    {
        await using var connection = _sqliteConnectionFactory.CreateConnection();
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = Queries.GetMediaSummaryEntries();
        command.Parameters.AddWithValue("search", $"%{search}%");
        command.Parameters.AddWithValue("pageSize", pageSize);
        command.Parameters.AddWithValue("lastId", lastId);
        await using var reader = await command.ExecuteReaderAsync();
        
        var results = new List<MediaSummary>();
        while (await reader.ReadAsync())
        {
            results.Add(new MediaSummary()
            {
                Id = reader.GetInt64(reader.GetOrdinal("Id")),
                Title = reader.GetString(reader.GetOrdinal("Title")),
                MediumType = Enum.Parse<MediumType>(reader.GetString(reader.GetOrdinal("Name"))),
                Stars = reader.GetDouble(reader.GetOrdinal("Stars"))
            });
        }
        return results;
    }

    public async Task<Media?> GetMediaAsync(string id)
    {
        await using var connection = _sqliteConnectionFactory.CreateConnection();
        await connection.OpenAsync();

        long mediaId;
        string title;
        MediumType medium;
        string? subMedium;
        string? description;

        await using (var command = connection.CreateCommand())
        {
            command.CommandText = Queries.GetMediaEntry();
            command.Parameters.AddWithValue("id", id);

            await using var reader = await command.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
            {
                return null;
            }

            mediaId = reader.GetInt64(reader.GetOrdinal("Id"));
            title = reader.GetString(reader.GetOrdinal("Title"));
            medium = Enum.Parse<MediumType>(
                reader.GetString(reader.GetOrdinal("Medium")));

            var subMediumOrdinal = reader.GetOrdinal("SubMedium");
            subMedium = reader.IsDBNull(subMediumOrdinal)
                ? null
                : reader.GetString(subMediumOrdinal);

            var descriptionOrdinal = reader.GetOrdinal("Description");
            description = reader.IsDBNull(descriptionOrdinal)
                ? null
                : reader.GetString(descriptionOrdinal);
        }

        var review = await GetMediaReviewAsync(connection, id);

        if (review is null)
        {
            return null;
        }

        return new Media
        {
            Id = mediaId,
            Title = title,
            Medium = medium,
            SubMedium = subMedium,
            Genres = await GetMediaGenresAsync(connection, id),
            Description = description,
            Review = review,
            ConsumptionHistory = await GetMediaConsumptionHistoryAsync(connection, id)
        };
    }
    
    public async Task<List<Genre>> GetMediaGenresAsync(string id)
    {
        await using var connection = _sqliteConnectionFactory.CreateConnection();
        await connection.OpenAsync();
        return await GetMediaGenresAsync(connection, id);
    }
    private static async Task<List<Genre>> GetMediaGenresAsync(SqliteConnection connection, string id)
    {
        List<Genre> genres = [];
        await using var command = connection.CreateCommand();
        command.CommandText = Queries.GetMediaGenres();
        command.Parameters.AddWithValue("id", id);
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            genres.Add(new Genre()
            {
                Id = reader.GetInt64(reader.GetOrdinal("Id")),
                Name = reader.GetString(reader.GetOrdinal("Name")),
            });
        }

        return genres;
    }

    public async Task<Review?> GetMediaReviewAsync(string id)
    {
        await using var connection = _sqliteConnectionFactory.CreateConnection();
        await connection.OpenAsync();
        return await GetMediaReviewAsync(connection, id);
    }
    private async Task<Review?> GetMediaReviewAsync(
        SqliteConnection connection,
        string id)
    {
        double stars;
        string? thoughts;

        await using (var command = connection.CreateCommand())
        {
            command.CommandText = Queries.GetMediaReview();
            command.Parameters.AddWithValue("id", id);

            await using var reader = await command.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
            {
                return null;
            }

            stars = reader.GetDouble(reader.GetOrdinal("Stars"));

            var thoughtsOrdinal = reader.GetOrdinal("Thoughts");
            thoughts = reader.IsDBNull(thoughtsOrdinal)
                ? null
                : reader.GetString(thoughtsOrdinal);
        }

        return new Review
        {
            Stars = stars,
            Thoughts = thoughts,
            Pros = await GetMediaReviewProsAsync(connection, id),
            Cons = await GetMediaReviewConsAsync(connection, id)
        };
    }

    private async Task<List<string>> GetMediaReviewConsAsync(SqliteConnection connection, string id)
    {
        List<string> cons = [];
        await using var command = connection.CreateCommand();
        command.CommandText = Queries.GetMediaReviewCons();
        command.Parameters.AddWithValue("id", id);
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            cons.Add(reader.GetString(reader.GetOrdinal("Description")));
        }

        return cons;
    }

    private async Task<List<string>> GetMediaReviewProsAsync(SqliteConnection connection, string id)
    {
        List<string> pros = [];
        await using var command = connection.CreateCommand();
        command.CommandText = Queries.GetMediaReviewPros();
        command.Parameters.AddWithValue("id", id);
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            pros.Add(reader.GetString(reader.GetOrdinal("Description")));
        }

        return pros;
    }

    public async Task<List<DateTime>> GetMediaConsumptionHistoryAsync(string id)
    {
        await using var connection = _sqliteConnectionFactory.CreateConnection();
        await connection.OpenAsync();
        return await GetMediaConsumptionHistoryAsync(connection, id);
    }
    private async Task<List<DateTime>> GetMediaConsumptionHistoryAsync(SqliteConnection connection, string id)
    {
        List<DateTime> consumptionHistory = [];
        await using var command = connection.CreateCommand();
        command.CommandText = Queries.GetMediaConsumptionHistory();
        command.Parameters.AddWithValue("id", id);
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            consumptionHistory.Add(reader.GetDateTime(reader.GetOrdinal("ConsumedAt")));
        }
        return  consumptionHistory;
    }
    
    public async Task DeleteMediaAsync(string id)
    {
        await using var connection = _sqliteConnectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = Queries.DeleteMediaEntry();
        command.Parameters.AddWithValue("id", id);

        await command.ExecuteNonQueryAsync();
    }
}
