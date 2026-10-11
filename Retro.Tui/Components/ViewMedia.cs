using Retro.Core.Data;
using Retro.Infrastructure.Database;
using Retro.Tui.Data.Config;
using Retro.Tui.Data.Enums;
using Spectre.Console;

namespace Retro.Tui.Components;

public class ViewMedia(IMediaRepository repository)
{
    private readonly List<MenuOption<MediaViewOption>> _options = [
        new(MediaViewOption.Edit, "Edit media"),
        new(MediaViewOption.Delete, "Delete media"),
        new(MediaViewOption.Back, "Back")
    ];
    private bool _continueListing = true;
    
    public async Task ViewMediaEntryAsync(long mediaId)
    {
        try
        {
            while (_continueListing)
            {
                await Run(mediaId);
            }
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]An error has occurred: {Markup.Escape(ex.Message)}[/]");
            AnsiConsole.WriteLine("Press any key to return to the list...");
            Console.ReadKey();
        }
    }

    private async Task Run(long mediaId)
    {
        AnsiConsole.Clear();

        RetroHeader.GenerateHeader();

        var media = await repository.GetMediaAsync(mediaId.ToString());

        if (media is null)
        {
            AnsiConsole.MarkupLine($"[red]No media found with ID {mediaId}[/]");
            AnsiConsole.WriteLine("Press any key to return to the list...");
            Console.ReadKey();
            return;
        }

        var table = new Table();
        table.AddColumn("Property");
        table.AddColumn("Value");

        table.AddRow("Id", media.Id.ToString()!);
        table.AddRow("Title", Markup.Escape(media.Title));
        table.AddRow("Medium", Markup.Escape(media.Medium.ToString()));
        table.AddRow("Rating", media.Review.Stars.ToString("F1"));

        AnsiConsole.Write(table);
        
        AnsiConsole.MarkupLine(
            "[grey]E[/] [bold]Edit[/]    " +
            "[grey]D[/] [bold]Delete[/]    " +
            "[grey]Esc[/] [bold]Back[/]"
        );
        var key = Console.ReadKey(intercept: true);

        var input = key.Key switch
        {
            ConsoleKey.E => _options.Single(x => x.Operation == MediaViewOption.Edit),
            ConsoleKey.D => _options.Single(x => x.Operation == MediaViewOption.Delete),
            ConsoleKey.Escape => _options.Single(x => x.Operation == MediaViewOption.Back),
            _ => null
        };

        if (input is not null)
        {
            await HandleUserSelection(input, media);
        }
    }
    
    private async Task HandleUserSelection(MenuOption<MediaViewOption> input, Media data)
    {
        switch (input.Operation)
        {
            case MediaViewOption.Edit:
                break;
            case MediaViewOption.Delete:
                AnsiConsole.MarkupLine($"[red]Are you sure you want to delete '{Markup.Escape(data.Title)}'? This action cannot be undone. Y/N[/]");
                var response = Console.ReadKey(intercept: true);
                if (response.Key == ConsoleKey.Y)
                {
                    await repository.DeleteMediaAsync(data.Id.ToString()!);
                    _continueListing = false;
                }
                break;
            case MediaViewOption.Back:
                _continueListing = false;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

}