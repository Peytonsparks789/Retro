namespace Retro.Infrastructure.Database;

public static class Queries
{
    public static string MediaDbTablesInit(string args)
        => $"""
            -- Base Tables
            CREATE TABLE IF NOT EXISTS Mediums(
                Id INTEGER PRIMARY KEY,
                Name TEXT NOT NULL UNIQUE
            );
            
            CREATE TABLE IF NOT EXISTS SubMediums(
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL UNIQUE
            );
            
            CREATE TABLE IF NOT EXISTS Genres(
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL UNIQUE
            );
            
            CREATE TABLE IF NOT EXISTS Media(
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Title TEXT NOT NULL,
                MediumId INTEGER NOT NULL,
                SubMediumId INTEGER,
                Description TEXT NOT NULL,

                FOREIGN KEY (MediumId) REFERENCES Mediums(Id),
                FOREIGN KEY (SubMediumId) REFERENCES SubMediums(Id)
            );

            -- Data Inserts
            {args}

            -- Contextual Tables
            CREATE TABLE IF NOT EXISTS MediaGenres(
                MediaId INTEGER NOT NULL,
                GenreId INTEGER NOT NULL,

                PRIMARY KEY (MediaId, GenreId),
                FOREIGN KEY (MediaId) REFERENCES Media(Id) ON DELETE CASCADE,
                FOREIGN KEY (GenreId) REFERENCES Genres(Id)
            );

            CREATE TABLE IF NOT EXISTS MediaReviews(
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                MediaId INTEGER NOT NULL UNIQUE,
                Stars FLOAT NOT NULL,
                Thoughts TEXT NULL,

                FOREIGN KEY (MediaId) REFERENCES Media(Id) ON DELETE CASCADE
            );

            CREATE TABLE IF NOT EXISTS MediaReviewPros(
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                MediaReviewId INTEGER NOT NULL,
                Description TEXT NOT NULL,
                
                UNIQUE(MediaReviewId, Description),

                FOREIGN KEY (MediaReviewId) REFERENCES MediaReviews(Id) ON DELETE CASCADE
            );

            CREATE TABLE IF NOT EXISTS MediaReviewCons(
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                MediaReviewId INTEGER NOT NULL,
                Description TEXT NOT NULL,
                
                UNIQUE(MediaReviewId, Description),

                FOREIGN KEY (MediaReviewId) REFERENCES MediaReviews(Id) ON DELETE CASCADE
            );

            CREATE TABLE IF NOT EXISTS MediaConsumptionHistory(
                MediaId INTEGER NOT NULL,
                ConsumedAt TEXT NOT NULL,

                PRIMARY KEY (MediaId, ConsumedAt),
                FOREIGN KEY (MediaId) REFERENCES Media(Id) ON DELETE CASCADE
            );
            
            DELETE FROM Genres
            WHERE NOT EXISTS (
                SELECT 1
                FROM MediaGenres
                WHERE MediaGenres.GenreId = Genres.Id
            );
            
            DELETE FROM SubMediums
            WHERE NOT EXISTS (
                SELECT 1
                FROM Media
                WHERE Media.SubMediumId = SubMediums.Id
            );

            """;

    public static string UpsertSubMediumEntry()
        => """
           INSERT INTO SubMediums (Name)
           VALUES ($name)
           ON CONFLICT(Name) DO UPDATE SET Name = excluded.Name
           RETURNING Id;
           """;

    public static string InsertGenreEntry()
        => """
           INSERT INTO Genres (Name)
           VALUES ($name)
           ON CONFLICT(Name) DO UPDATE SET Name = excluded.Name
           RETURNING Id;
           """;
    
    public static string UpdateGenreEntry()
        => """
           UPDATE Genres
           SET Name = $name
           WHERE Id = $id
           RETURNING Id;
           """;
    
    public static string UpsertMediaEntry()
        => """
           INSERT INTO Media (Id, Title, MediumId, SubMediumId, Description)
           VALUES ($id, $title, $mediumId, $subMediumId, $description)
           ON CONFLICT(Id) DO UPDATE SET Title =  $title, MediumId = $mediumId, SubMediumId = $subMediumId,  Description = $description
           RETURNING Id;
           """;

    public static string InsertMediaEntry()
        => """
           INSERT INTO Media (Title, MediumId, SubMediumId, Description)
           VALUES ($title, $mediumId, $subMediumId, $description)
           RETURNING Id;
           """;

    public static string UpsertMediaGenresEntry()
        => """
           INSERT INTO MediaGenres (MediaId, GenreId)
           VALUES ($mediaId, $genreId)
           ON CONFLICT(MediaId, GenreId) DO NOTHING;
           """;

    public static string UpsertMediaReviewEntry()
        => """
           INSERT INTO MediaReviews (MediaId, Stars, Thoughts)
           VALUES ($mediaId, $stars, $thoughts)
           ON CONFLICT(MediaId) DO UPDATE SET Stars = $stars, Thoughts = $thoughts
           RETURNING Id;
           """;

    public static string UpsertMediaReviewProsEntry()
        => """
           INSERT INTO MediaReviewPros (MediaReviewId, Description)
           VALUES ($mediaReviewId, $description)
           ON CONFLICT(MediaReviewId, Description) DO UPDATE SET mediaReviewId = excluded.MediaReviewId, Description = $description;
           """;

    public static string UpsertMediaReviewConsEntry()
        => """
           INSERT INTO MediaReviewCons (MediaReviewId, Description)
           VALUES ($mediaReviewId, $description)
           ON CONFLICT(MediaReviewId, Description) DO UPDATE SET mediaReviewId = excluded.MediaReviewId, Description = $description;
           """;

    public static string UpsertMediaConsumptionHistoryEntry()
        => """
           INSERT INTO MediaConsumptionHistory (MediaId, ConsumedAt)
           VALUES ($mediaId, $consumedAt)
           ON CONFLICT(MediaId, ConsumedAt) DO NOTHING;
           """;

    public static string GetMediaSummaryEntries()
        => """
           SELECT Media.Id, Media.Title, Mediums.Name, MediaReviews.Stars
           FROM Media
           INNER JOIN Mediums
               ON Media.MediumId = Mediums.Id
           INNER JOIN MediaReviews
               ON Media.Id = MediaReviews.MediaId
           WHERE
               (Media.Title LIKE $search
               OR Media.Description LIKE $search
               OR MediaReviews.Thoughts LIKE $search
               OR EXISTS (
                   SELECT 1
                   FROM MediaGenres
                   INNER JOIN Genres
                       ON MediaGenres.GenreId = Genres.Id
                   WHERE MediaGenres.MediaId = Media.Id
                     AND Genres.Name LIKE $search
               ))
               AND Media.Id > $lastId
           ORDER BY Media.Id
           LIMIT $pageSize;
           """;

    public static string GetMediaEntry()
        => """
           SELECT
               Media.Id,
               Media.Title,
               Mediums.Name AS Medium,
               SubMediums.Name AS SubMedium,
               Media.Description
           FROM Media
           INNER JOIN Mediums
               ON  Media.MediumId = Mediums.Id
           LEFT JOIN SubMediums
               ON Media.SubMediumId = SubMediums.Id
           WHERE Media.Id = $id;
           """;

    public static string GetMediaGenres()
        => """
           SELECT Genres.Id, Genres.Name
           FROM MediaGenres
           INNER JOIN Genres
               ON MediaGenres.GenreId = Genres.Id
           WHERE MediaGenres.MediaId = $id;
           """;

    public static string GetMediaReview()
        => """
           SELECT Stars, Thoughts
           FROM MediaReviews
           WHERE MediaReviews.MediaId = $id;
           """;

    public static string GetMediaReviewPros()
        => """
           SELECT Description
           FROM MediaReviews
           INNER JOIN MediaReviewPros
               ON MediaReviews.Id = MediaReviewPros.MediaReviewId
           WHERE MediaReviews.MediaId = $id;
           """;
    
    public static string GetMediaReviewCons()
        => """
           SELECT Description
           FROM MediaReviews
           INNER JOIN MediaReviewCons
               ON MediaReviews.Id = MediaReviewCons.MediaReviewId
           WHERE MediaReviews.MediaId = $id;
           """;

    public static string GetMediaConsumptionHistory()
        => """
           SELECT ConsumedAt
           FROM MediaConsumptionHistory
           WHERE MediaConsumptionHistory.MediaId = $id
           ORDER BY  ConsumedAt;
           """;

    public static string DeleteMediaEntry()
        => """
           DELETE FROM Media
           WHERE Id = $id;
           """;
}