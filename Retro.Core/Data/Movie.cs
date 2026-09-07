namespace Retro.Core.Data;

public class Movie : Media
{
    public required MovieData MediumData { get; set; }
}

public class MovieData
{
    public required MovieCreators Creators { get; set; }
    
    public DateOnly ReleaseDate { get; set; }
    
    public int RuntimeMinutes { get; set; }
}

public class MovieCreators
{
    public List<string>? Directors { get; set; } = [];
    
    public List<string>? Writers { get; set; } = [];
    
    public List<string>? Producers { get; set; } = [];
    
    public List<string>? Cast { get; set; } = [];
}