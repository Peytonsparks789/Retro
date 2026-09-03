using System.Text.Json.Serialization;
using Retro.Core.Enums;

namespace Retro.Core.Data;

public class Media
{
    [JsonPropertyName("id")]
    public required string Id { get; set; }
    
    [JsonPropertyName("title")]
    public required string Title { get; set; }
    
    [JsonPropertyName("medium")]
    public required MediumType Medium { get; set; }

    [JsonPropertyName("subMedium")]
    public string? SubMedium { get; set; }

    [JsonPropertyName("genres")]
    public List<string> Genres { get; set; } = [];
    
    [JsonPropertyName("description")]
    public string? Description { get; set; }
    
    [JsonPropertyName("review")]
    public required Review Review { get; set; }
    
    [JsonPropertyName("consumption")]
    public required Consumption Consumption { get; set; }
}

public abstract class Review
{
    [JsonPropertyName("stars")]
    public required double Stars { get; set; }
    
    [JsonPropertyName("thoughts")]
    public string? Thoughts { get; set; }

    [JsonPropertyName("pros")]
    public List<string> Pros { get; set; } = [];
    
    [JsonPropertyName("cons")]
    public List<string> Cons { get; set; } = [];
}

public abstract class Consumption
{
    [JsonPropertyName("firsConsumedDate")]
    public DateTime? FirstConsumedDate { get; set; }
    
    [JsonPropertyName("lastConsumedDate")]
    public DateTime? LastConsumedDate { get; set; }
    
    [JsonPropertyName("total")]
    public int TimesConsumed { get; set; }
}