using Microsoft.Data.Sqlite;
using Retro.Core.Data;
using Retro.Core.Enums;

namespace Retro.Infrastructure.Database;

public class MediaRepository(ISqliteConnectionFactory connectionFactory) : IMediaRepository
{
    public async Task UpsertMediaAsync(Media media)
    {
        ArgumentNullException.ThrowIfNull(media);
        
        await using var connection = connectionFactory.CreateConnection();
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
                if (media.Id is null)
                {
                    command.CommandText = Queries.InsertMediaEntry();
                }
                else
                {
                    command.CommandText = Queries.UpdateMediaEntry();
                    command.Parameters.AddWithValue("id", media.Id);
                }
                command.Parameters.AddWithValue("title", media.Title);
                command.Parameters.AddWithValue("mediumId", (int)media.Medium);
                command.Parameters.AddWithValue("subMediumId", (object?)subMediumId ?? DBNull.Value);
                command.Parameters.AddWithValue("description", (object?)media.Description ?? DBNull.Value);
                mediaId = (long?)command.ExecuteScalar();
                ArgumentNullException.ThrowIfNull(mediaId);
                media.Id = mediaId;
            }

            var dbMediaGenresPreUpsert = await GetMediaGenresAsync(
                connection,
                media.Id.ToString()!,
                transaction);
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
                genre.Id = genreId;
                
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
            var staleMediaGenres = dbMediaGenresPreUpsert
                .Where(dbItem => media.Genres.All(x => x.Id != dbItem.Id))
                .ToList();
            foreach (var mediaGenre in staleMediaGenres)
            {
                await using var command = connection.CreateCommand();
                command.Transaction = transaction;
                command.CommandText = Queries.DeleteMediaGenresEntry();
                command.Parameters.AddWithValue("mediaId", mediaId);
                command.Parameters.AddWithValue("genreId", mediaGenre.Id);
                command.ExecuteNonQuery();
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
            var dbMediaProsPreUpsert = await GetMediaReviewProsAsync(
                connection,
                media.Id.ToString()!,
                transaction);
            foreach (var pro in media.Review.Pros)
            {
                await using var command = connection.CreateCommand();
                command.Transaction = transaction;
                if (pro.Id is null)
                {
                    command.CommandText = Queries.InsertMediaReviewProsEntry();
                    command.Parameters.AddWithValue("mediaReviewId", reviewId);
                }
                else
                {
                    command.CommandText = Queries.UpdateMediaReviewProsEntry();
                    command.Parameters.AddWithValue("id", pro.Id);
                }
                command.Parameters.AddWithValue("description", pro.Description);
                var proId = (long?)command.ExecuteScalar();
                ArgumentNullException.ThrowIfNull(proId);
                pro.Id = proId;
            }
            var staleMediaReviewPros = dbMediaProsPreUpsert
                .Where(dbItem => media.Review.Pros.All(x => x.Id != dbItem.Id))
                .ToList();
            foreach (var pro in staleMediaReviewPros)
            {
                await using var command = connection.CreateCommand();
                command.Transaction = transaction;
                command.CommandText = Queries.DeleteMediaReviewProsEntry();
                command.Parameters.AddWithValue("id", pro.Id);
                command.ExecuteNonQuery();
            }
            
            // Upsert MediaReviewCons table
            var dbMediaConsPreUpsert = await GetMediaReviewConsAsync(
                connection,
                media.Id.ToString()!,
                transaction);
            foreach (var con in media.Review.Cons)
            {
                await using var command = connection.CreateCommand();
                command.Transaction = transaction;
                if (con.Id is null)
                {
                    command.CommandText = Queries.InsertMediaReviewConsEntry();
                    command.Parameters.AddWithValue("mediaReviewId", reviewId);
                }
                else
                {
                    command.CommandText = Queries.UpdateMediaReviewConsEntry();
                    command.Parameters.AddWithValue("id", con.Id);
                }
                command.Parameters.AddWithValue("description", con.Description);
                var conId = (long?)command.ExecuteScalar();
                ArgumentNullException.ThrowIfNull(conId);
                con.Id = conId;
            }
            var staleMediaReviewCons = dbMediaConsPreUpsert
                .Where(dbItem => media.Review.Cons.All(x => x.Id != dbItem.Id))
                .ToList();
            foreach (var con in staleMediaReviewCons)
            {
                await using var command = connection.CreateCommand();
                command.Transaction = transaction;
                command.CommandText = Queries.DeleteMediaReviewConsEntry();
                command.Parameters.AddWithValue("id", con.Id);
                command.ExecuteNonQuery();
            }
            
            // Upsert MediaConsumptionHistory table
            var dbConsumptionHistoryPreUpsert = await GetMediaConsumptionHistoryAsync(
                connection,
                media.Id.ToString()!,
                transaction);
            foreach (var consumption in media.ConsumptionHistory)
            {
                await using var command = connection.CreateCommand();
                command.Transaction = transaction;
                command.CommandText = Queries.UpsertMediaConsumptionHistoryEntry();
                command.Parameters.AddWithValue("mediaId", mediaId);
                command.Parameters.AddWithValue("consumedAt", consumption.ConsumedAt);
                var consumptionId = (long?)command.ExecuteScalar();
                ArgumentNullException.ThrowIfNull(consumptionId);
                consumption.Id = consumptionId;
            }
            var staleConsumptionHistory = dbConsumptionHistoryPreUpsert
                .Where(dbItem => media.ConsumptionHistory.All(x => x.Id != dbItem.Id))
                .ToList();
            foreach (var consumedAt in staleConsumptionHistory)
            {
                await using var command = connection.CreateCommand();
                command.Transaction = transaction;
                command.CommandText = Queries.DeleteConsumptionHistoryEntry();
                command.Parameters.AddWithValue("id", consumedAt.Id);
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
        string? search,
        int pageSize,
        long lastId,
        bool sortAscending)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = Queries.GetMediaSummaryEntries(sortAscending: sortAscending);
        command.Parameters.AddWithValue(
            "$search",
            string.IsNullOrWhiteSpace(search)
                ? DBNull.Value
                : $"%{search}%");
        command.Parameters.AddWithValue("pageSize", pageSize + 1); 
        command.Parameters.AddWithValue("lastId", lastId <= 0 ? 0 : lastId);
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
        await using var connection = connectionFactory.CreateConnection();
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
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync();
        return await GetMediaGenresAsync(connection, id);
    }
    private static async Task<List<Genre>> GetMediaGenresAsync(
        SqliteConnection connection,
        string id,
        SqliteTransaction? transaction = null)
    {
        List<Genre> genres = [];
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
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
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync();
        return await GetMediaReviewAsync(connection, id);
    }
    private async Task<Review?> GetMediaReviewAsync(
        SqliteConnection connection,
        string id,
        SqliteTransaction? transaction = null)
    {
        double stars;
        string? thoughts;

        await using (var command = connection.CreateCommand())
        {
            command.Transaction = transaction;
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
            Pros = await GetMediaReviewProsAsync(connection, id, transaction),
            Cons = await GetMediaReviewConsAsync(connection, id, transaction)
        };
    }

    private async Task<List<ReviewCon>> GetMediaReviewConsAsync(
        SqliteConnection connection,
        string id,
        SqliteTransaction? transaction = null)
    {
        List<ReviewCon> cons = [];
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = Queries.GetMediaReviewCons();
        command.Parameters.AddWithValue("id", id);
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            cons.Add(
                new ReviewCon
                {
                    Id = reader.GetInt64(reader.GetOrdinal("Id")),
                    Description = reader.GetString(reader.GetOrdinal("Description"))
                });
        }

        return cons;
    }

    private async Task<List<ReviewPro>> GetMediaReviewProsAsync(
        SqliteConnection connection,
        string id,
        SqliteTransaction? transaction = null)
    {
        List<ReviewPro> pros = [];
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = Queries.GetMediaReviewPros();
        command.Parameters.AddWithValue("id", id);
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            pros.Add(                
                new ReviewPro
                {
                    Id = reader.GetInt64(reader.GetOrdinal("Id")),
                    Description = reader.GetString(reader.GetOrdinal("Description"))
                });
        }

        return pros;
    }

    public async Task<List<Consumption>> GetMediaConsumptionHistoryAsync(string id)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync();
        return await GetMediaConsumptionHistoryAsync(connection, id);
    }
    private async Task<List<Consumption>> GetMediaConsumptionHistoryAsync(
        SqliteConnection connection,
        string id,
        SqliteTransaction? transaction = null)
    {
        List<Consumption> consumptionHistory = [];
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = Queries.GetMediaConsumptionHistory();
        command.Parameters.AddWithValue("id", id);
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            consumptionHistory.Add(
                new Consumption()
                {
                    Id = reader.GetInt64(reader.GetOrdinal("Id")),
                    ConsumedAt = reader.GetDateTime(reader.GetOrdinal("ConsumedAt"))
                });
        }
        return  consumptionHistory;
    }
    
    public async Task DeleteMediaAsync(string id)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = Queries.DeleteMediaEntry();
        command.Parameters.AddWithValue("id", id);

        await command.ExecuteNonQueryAsync();
    }
}
