namespace Retro.Core.Data;

public class Book : Media
{
    public required BookData MediumData { get; set; }
}

public class BookData
{
    public required BookCreators Creators { get; set; }

    public DateOnly ReleaseDate { get; set; }

    public string? Isbn { get; set; }

    public string? Publisher { get; set; }

    public int? PageCount { get; set; }
}

public class BookCreators
{
    public List<string> Authors { get; set; } = [];

    public List<string> Editors { get; set; } = [];

    public List<string> Translators { get; set; } = [];
}