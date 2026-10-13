using Retro.Core.Data;
using Retro.Core.Enums;
using Retro.Infrastructure.Database;
using Spectre.Console;

namespace Retro.Tui.Components;

public class AddMedia(IMediaRepository repository) : LayoutComponent
{
    public override async Task RunAsync()
    {
        try
        {
            ResetDefaults();
            while (ContinueRunning)
            {
                var media = new Media()
                {
                    Medium = GetMediumType(),
                    Title = GetTitle(),
                    SubMedium = GetSubMedium(),
                    Genres = GetGenres(),
                    Description = GetDescription(),
                    Review = GetReview(),
                    ConsumptionHistory = GetConsumptionHistory()
                };

                var confirmation = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("Add media entry?")
                        .AddChoices("Yes", "No"));

                if (confirmation.Equals("Yes", StringComparison.OrdinalIgnoreCase))
                {
                    await repository.UpsertMediaAsync(media);
                }

                ContinueRunning = false;
            }
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]An error has occurred: {Markup.Escape(ex.Message)}[/]");
            AnsiConsole.WriteLine("Press any key to return to the list...");
            Console.ReadKey();
        }
    }
    
    private static MediumType GetMediumType()
    {
        var mediumType = AnsiConsole.Prompt(
            new SelectionPrompt<MediumType>()
                .Title("Select medium type")
                .AddChoices(Enum.GetValues<MediumType>()));
        AnsiConsole.MarkupLine($"[green]Medium: [/]{mediumType}");
        return mediumType;
    }

    private static string GetTitle()
        => AnsiConsole.Prompt(
            new TextPrompt<string>("[green]Title: [/]"));

    private static string? GetSubMedium()
    {
        var subMedium = AnsiConsole.Prompt(
            new TextPrompt<string>("[green]Sub Medium: [/]")
                .AllowEmpty());
        return string.IsNullOrWhiteSpace(subMedium)
            ? null
            : subMedium;
    }
    
    private static List<Genre> GetGenres()
    {
        var genres = new List<Genre>();
        while (true)
        {
            var genre = AnsiConsole.Prompt(
                new TextPrompt<string>("[green]Genre[/] [grey](or 'q' to finish)[green]: [/][/]"));

            if (!genre.Equals("q", StringComparison.OrdinalIgnoreCase))
            {
                genres.Add(new Genre{Name = genre});
            }
            if (genre.Equals("q", StringComparison.OrdinalIgnoreCase) && genres.Count != 0)
            {
                break;
            }
        }

        return genres;
    }

    private static string GetDescription()
        => AnsiConsole.Prompt(
            new TextPrompt<string>("[green]Description: [/]"));

    private static Review GetReview()
    {
        var stars = AnsiConsole.Prompt(
            new TextPrompt<double>("[green]Rating[/] [grey](1-5)[green]: [/][/]")
                .Validate(x =>
                    x is >= 0 and <= 5));
        var thoughts = AnsiConsole.Prompt(
            new TextPrompt<string>("[green]Thoughts: [/]")
                .AllowEmpty());

        thoughts = string.IsNullOrWhiteSpace(thoughts)
            ? null
            : thoughts;
        
        var pros = GetReviewPro();
        var cons = GetReviewCon();

        return new Review()
        {
            Stars = stars,
            Thoughts = thoughts,
            Pros = pros,
            Cons = cons
        };
    }

    private static List<ReviewPro> GetReviewPro()
    {
        var pros = new List<ReviewPro>();
        while (true)
        {
            var input = AnsiConsole.Prompt(
                new TextPrompt<string>("[green]Review Pro(s)[/] [grey](or 'q' to finish)[green]: [/][/]"));

            if (!input.Equals("q", StringComparison.OrdinalIgnoreCase))
            {
                pros.Add(new ReviewPro{Description = input});
            }
            if (input.Equals("q", StringComparison.OrdinalIgnoreCase) && pros.Count != 0)
            {
                break;
            }
        }

        return pros;
    }

    private static List<ReviewCon> GetReviewCon()
    {
        var cons = new List<ReviewCon>();
        while (true)
        {
            var input = AnsiConsole.Prompt(
                new TextPrompt<string>("[green]Review Con(s)[/] [grey](or 'q' to finish)[green]: [/][/]"));

            if (!input.Equals("q", StringComparison.OrdinalIgnoreCase))
            {
                cons.Add(new ReviewCon{Description = input});
            }
            if (input.Equals("q", StringComparison.OrdinalIgnoreCase) && cons.Count != 0)
            {
                break;
            }
        }

        return cons;
    }

    private static List<Consumption> GetConsumptionHistory()
    {
        var dates = new List<Consumption>();
        while (true)
        {

            var input = AnsiConsole.Prompt(
                new TextPrompt<string>("[green]Consumption date(s) [grey](yyyy-MM-dd) or 'q' to finish: [/][/]"));

            if (!input.Equals("q", StringComparison.OrdinalIgnoreCase) && DateTime.TryParse(input, out var date))
            {
                dates.Add(new Consumption{ConsumedAt = date});
            }
            if (input.Equals("q", StringComparison.OrdinalIgnoreCase) && dates.Count != 0)
            {
                break;
            }
        }

        return dates;
    }
}