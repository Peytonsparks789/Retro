using Retro.Core.Data;
using Retro.Infrastructure.Database;
using Retro.Tui.Data.Config;
using Retro.Tui.Data.Enums;
using Spectre.Console;

namespace Retro.Tui.Components;

public class ViewMedia(
    KeybindMenuRenderer<MediaViewOptionsEnum> options,
    IMediaRepository repository) : LayoutComponent<ViewMediaOptions>
{
    private readonly List<MenuOption<MediaViewOptionsEnum>> _options = [
        new(MediaViewOptionsEnum.Edit, "Edit media", ConsoleKey.E, "E"),
        new(MediaViewOptionsEnum.Delete, "Delete media", ConsoleKey.D, "D"),
        new(MediaViewOptionsEnum.Back, "Back", ConsoleKey.Escape, "Esc")
    ];
    
    public override async Task RunAsync(ViewMediaOptions request)
    {
        try
        {
            ResetDefaults();
            while (ContinueRunning)
            {
                AnsiConsole.Clear();

                RetroHeader.GenerateHeader();

                var data = await repository.GetMediaAsync(request.Id.ToString());

                if (data is null)
                {
                    AnsiConsole.MarkupLine($"[red]No media found with ID {request.Id}[/]");
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

                var input = await options.RunAsync(_options); 
                await HandleUserSelection(input, data);
            }
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]An error has occurred: {Markup.Escape(ex.Message)}[/]");
            AnsiConsole.WriteLine("Press any key to return to the list...");
            Console.ReadKey();
        }
    }
    
    private async Task HandleUserSelection(MenuOption<MediaViewOptionsEnum> input, Media data)
    {
        switch (input.Operation)
        {
            case MediaViewOptionsEnum.Edit:
                break;
            case MediaViewOptionsEnum.Delete:
                AnsiConsole.MarkupLine($"[red]Are you sure you want to delete '{Markup.Escape(data.Title)}'? This action cannot be undone. Y/N[/]");
                var response = Console.ReadKey(intercept: true);
                if (response.Key == ConsoleKey.Y)
                {
                    await repository.DeleteMediaAsync(data.Id.ToString()!);
                    ContinueRunning = false;
                }
                break;
            case MediaViewOptionsEnum.Back:
                ContinueRunning = false;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}