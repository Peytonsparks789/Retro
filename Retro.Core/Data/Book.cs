using System.Text.Json.Serialization;

namespace Retro.Core.Data;

public class Book : Media
{
    [JsonPropertyName("mediumData")]
    public required BookData MediumData { get; set; }
}

public abstract class BookData
{
    [JsonPropertyName("creators")]
    public required BookCreators Creators { get; set; }

    [JsonPropertyName("releaseDate")]
    public DateOnly ReleaseDate { get; set; }

    [JsonPropertyName("isbn")]
    public string? Isbn { get; set; }

    [JsonPropertyName("publisher")]
    public string? Publisher { get; set; }

    [JsonPropertyName("pageCount")]
    public int? PageCount { get; set; }
}

public abstract class BookCreators
{
    [JsonPropertyName("authors")]
    public List<string> Authors { get; set; } = [];

    [JsonPropertyName("editors")]
    public List<string> Editors { get; set; } = [];

    [JsonPropertyName("translators")]
    public List<string> Translators { get; set; } = [];
}