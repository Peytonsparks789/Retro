using Retro.Core.Enums;

namespace Retro.Core.Data;

public class MediaSummary
{
    public required long Id { get; set; }
    public required string Title { get; set; }
    public required MediumType MediumType { get; set; }
    public required double Stars { get; set; }
}