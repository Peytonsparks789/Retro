using Retro.Tui.Data.Config;
using Spectre.Console;
using Retro.Tui.Data.Enums;

namespace Retro.Tui.Components;

public class MainMenu(
    AddMedia addMedia,
    ListMedia listMedia
    )
{
    private readonly List<MenuOption<MainMenuOption>> _options = [
        new(MainMenuOption.Add, "Add Media"),
        new(MainMenuOption.List, "List Media"),
        new(MainMenuOption.Exit, "Exit")
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
            
        var selection = AnsiConsole.Prompt(
            new SelectionPrompt<MenuOption<MainMenuOption>>()
                .Title("Welcome to Retro, what would you like to do?")
                .AddChoices(_options)
                .UseConverter(option => option.Label));

        switch (selection.Operation)
        {
            case MainMenuOption.Add:
                await addMedia.AddMediaEntryAsync();
                break;
            case MainMenuOption.List:
                await listMedia.ListMediaEntriesAsync();
                break;
            case MainMenuOption.Exit:
                _continueRunning = false;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}