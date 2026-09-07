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
                FOREIGN KEY (MediaId) REFERENCES Media(Id),
                FOREIGN KEY (GenreId) REFERENCES Genres(Id)
            );

            CREATE TABLE IF NOT EXISTS MediaReviews(
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                MediaId INTEGER NOT NULL UNIQUE,
                Stars FLOAT NOT NULL,
                Thoughts TEXT NULL,

                FOREIGN KEY (MediaId) REFERENCES Media(Id)
            );

            CREATE TABLE IF NOT EXISTS MediaReviewPros(
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                MediaReviewId INTEGER NOT NULL,
                Description TEXT NOT NULL,

                FOREIGN KEY (MediaReviewId) REFERENCES MediaReviews(Id)
            );

            CREATE TABLE IF NOT EXISTS MediaReviewCons(
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                MediaReviewId INTEGER NOT NULL,
                Description TEXT NOT NULL,

                FOREIGN KEY (MediaReviewId) REFERENCES MediaReviews(Id)
            );

            CREATE TABLE IF NOT EXISTS MediaConsumptionHistory(
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                MediaId INTEGER NOT NULL,
                ConsumedAt TEXT NOT NULL,

                FOREIGN KEY (MediaId) REFERENCES Media(Id)
            );

            """;
}