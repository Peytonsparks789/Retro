using Retro.Core.Data;
using Retro.Infrastructure.Database;
using Retro.Tui.Data.Config;
using Retro.Tui.Data.Enums;
using Spectre.Console;

namespace Retro.Tui.Components;

public class ViewMedia(
    MenuOptionsRenderer<MediaViewOptions> options,
    IMediaRepository repository)
{
    private readonly List<MenuOption<MediaViewOptions>> _options = [
        new(MediaViewOptions.Edit, "Edit media", ConsoleKey.E, "E"),
        new(MediaViewOptions.Delete, "Delete media", ConsoleKey.D, "D"),
        new(MediaViewOptions.Back, "Back", ConsoleKey.Escape, "Esc")
    ];
    private bool _continueListing = true;
    
    public async Task ViewMediaEntryAsync(long mediaId)
    {
        ResetDefaults();
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

        var data = await repository.GetMediaAsync(mediaId.ToString());

        if (data is null)
        {
            AnsiConsole.MarkupLine($"[red]No media found with ID {mediaId}[/]");
            AnsiConsole.WriteLine("Press any key to return to the list...");
            Console.ReadKey();
            return;
        }

        var table = new Table();
        table.AddColumn("Property");
        table.AddColumn("Value");

        table.AddRow("Id", data.Id.ToString()!);
        table.AddRow("Title", Markup.Escape(data.Title));
        table.AddRow("Medium", Markup.Escape(data.Medium.ToString()));
        table.AddRow("Rating", data.Review.Stars.ToString("F1"));

        AnsiConsole.Write(table);

        var input = options.RenderAsync(_options); 
        await HandleUserSelection(input, data);
    }
    
    private async Task HandleUserSelection(MenuOption<MediaViewOptions> input, Media data)
    {
        switch (input.Operation)
        {
            case MediaViewOptions.Edit:
                break;
            case MediaViewOptions.Delete:
                AnsiConsole.MarkupLine($"[red]Are you sure you want to delete '{Markup.Escape(data.Title)}'? This action cannot be undone. Y/N[/]");
                var response = Console.ReadKey(intercept: true);
                if (response.Key == ConsoleKey.Y)
                {
                    await repository.DeleteMediaAsync(data.Id.ToString()!);
                    _continueListing = false;
                }
                break;
            case MediaViewOptions.Back:
                _continueListing = false;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void ResetDefaults()
    {
        _continueListing = true;
    }
}