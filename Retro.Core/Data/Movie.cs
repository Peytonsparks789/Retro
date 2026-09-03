using System.Text.Json.Serialization;

namespace Retro.Core.Data;

public class Movie : Media
{
    [JsonPropertyName("mediumData")]
    public required MovieData MediumData { get; set; }
}

public abstract class MovieData
{
    [JsonPropertyName("creators")]
    public required MovieCreators Creators { get; set; }
    
    [JsonPropertyName("releaseDate")]
    public DateOnly ReleaseDate { get; set; }
    
    [JsonPropertyName("rumTimeMinutes")]
    public int RuntimeMinutes { get; set; }
}

public abstract class MovieCreators
{
    [JsonPropertyName("directors")]
    public List<string>? Directors { get; set; } = [];
    
    [JsonPropertyName("writers")]
    public List<string>? Writers { get; set; } = [];
    
    [JsonPropertyName("producers")]
    public List<string>? Producers { get; set; } = [];
    
    [JsonPropertyName("cast")]
    public List<string>? Cast { get; set; } = [];
}
