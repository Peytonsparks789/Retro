using System.Text.Json.Serialization;

namespace Retro.Core.Data;

public class Music : Media
{
    [JsonPropertyName("mediumData")]
    public required MusicData MediumData { get; set; }
}

public abstract class MusicData
{
    [JsonPropertyName("creators")]
    public required MusicCreators Creators { get; set; }

    [JsonPropertyName("releaseDate")]
    public DateOnly ReleaseDate { get; set; }

    [JsonPropertyName("trackCount")]
    public int? TrackCount { get; set; }

    [JsonPropertyName("runtimeMinutes")]
    public int? RuntimeMinutes { get; set; }

    [JsonPropertyName("label")]
    public string? Label { get; set; }
}

public abstract class MusicCreators
{
    [JsonPropertyName("artists")]
    public List<string> Artists { get; set; } = [];

    [JsonPropertyName("producers")]
    public List<string> Producers { get; set; } = [];

    [JsonPropertyName("songwriters")]
    public List<string> Songwriters { get; set; } = [];

    [JsonPropertyName("composers")]
    public List<string> Composers { get; set; } = [];
}