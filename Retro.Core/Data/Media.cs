using Retro.Core.Enums;

namespace Retro.Core.Data;

public class Media
{
    public long? Id { get; set; }
    public required string Title { get; set; }
    
    public required MediumType Medium { get; set; }

    public string? SubMedium { get; set; }

    public List<Genre> Genres { get; set; } = [];
    
    public string? Description { get; set; }
    
    public required Review Review { get; set; }

    public required List<Consumption> ConsumptionHistory { get; set; } = [];
}

public class Review
{
    public required double Stars { get; set; }
    
    public string? Thoughts { get; set; }

    public List<ReviewPro> Pros { get; set; } = [];
    
    public List<ReviewCon> Cons { get; set; } = [];
}

public class Genre
{
    public long? Id { get; set; }
    public required string Name { get; set; }
}

public class ReviewPro
{
    public long? Id { get; set; }
    public required string Description { get; set; }
}

public class ReviewCon
{
    public long? Id { get; set; }
    public required string Description { get; set; }
}

public class Consumption
{
    public long? Id { get; set; }
    public required DateTime ConsumedAt { get; set; }
}