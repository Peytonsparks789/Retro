using Retro.Tui.Data.Config;
using Retro.Tui.Data.Enums;
using Spectre.Console;

namespace Retro.Tui.Components;

public class MainMenu(
    MenuOptionsRenderer<MainMenuOptions> options,
    AddMedia addMedia,
    ListMedia listMedia
    )
{
    private readonly List<MenuOption<MainMenuOptions>> _options = [
        new(MainMenuOptions.Add, "Add Media", ConsoleKey.A, "A"),
        new(MainMenuOptions.List, "List Media", ConsoleKey.L, "L"),
        new(MainMenuOptions.Exit, "Exit", ConsoleKey.Escape, "Esc")
    ];
    private bool _continueRunning = true;
    
    public async Task RunAsync()
    {
        try
        {
            while (_continueRunning)
            {
                await Run();
            }
        }
        catch (Exception e)
        {
            AnsiConsole.MarkupLine($"[red]An unexpected error occurred: {Markup.Escape(e.Message)}[/]");
            throw;
        }
    }

    private async Task Run()
    {
        AnsiConsole.Clear();

        RetroHeader.GenerateHeader();
            
        AnsiConsole.MarkupLine("[green]Welcome to Retro![/]");

        var input = options.RenderAsync(_options);
        await HandleUserSelection(input);
    }
    
    private async Task HandleUserSelection(MenuOption<MainMenuOptions> selection)
    {
        switch (selection.Operation)
        {
            case MainMenuOptions.Add:
                await addMedia.AddMediaEntryAsync();
                break;
            case MainMenuOptions.List:
                await listMedia.ListMediaEntriesAsync();
                break;
            case MainMenuOptions.Exit:
                _continueRunning = false;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}