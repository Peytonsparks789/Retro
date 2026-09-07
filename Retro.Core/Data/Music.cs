namespace Retro.Core.Data;

public class Music : Media
{
    public required MusicData MediumData { get; set; }
}

public class MusicData
{
    public required MusicCreators Creators { get; set; }

    public DateOnly ReleaseDate { get; set; }

    public int? TrackCount { get; set; }

    public int? RuntimeMinutes { get; set; }

    public string? Label { get; set; }
}

public class MusicCreators
{
    public List<string> Artists { get; set; } = [];

    public List<string> Producers { get; set; } = [];

    public List<string> Songwriters { get; set; } = [];

    public List<string> Composers { get; set; } = [];
}