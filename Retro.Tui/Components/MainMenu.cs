using Retro.Tui.Data.Config;
using Retro.Tui.Data.Enums;
using Spectre.Console;

namespace Retro.Tui.Components;

public class MainMenu(
    KeybindMenuRenderer<MainMenuOptionsEnum> options,
    AddMedia addMedia,
    ListMedia listMedia
    ) : LayoutComponent
{
    private readonly List<MenuOption<MainMenuOptionsEnum>> _options = [
        new(MainMenuOptionsEnum.Add, "Add Media", ConsoleKey.A, "A"),
        new(MainMenuOptionsEnum.List, "List Media", ConsoleKey.L, "L"),
        new(MainMenuOptionsEnum.Exit, "Exit", ConsoleKey.Escape, "Esc")
    ];
    
    public override async Task RunAsync()
    {
        try
        {
            ResetDefaults();
            while (ContinueRunning)
            {
                AnsiConsole.Clear();

                RetroHeader.GenerateHeader();
            
                AnsiConsole.MarkupLine("[green]Welcome to Retro![/]");

                var input = await options.RunAsync(_options);
                await HandleUserSelection(input);
            }
        }
        catch (Exception e)
        {
            AnsiConsole.MarkupLine($"[red]An unexpected error occurred: {Markup.Escape(e.Message)}[/]");
            throw;
        }
    }
    
    private async Task HandleUserSelection(MenuOption<MainMenuOptionsEnum> selection)
    {
        switch (selection.Operation)
        {
            case MainMenuOptionsEnum.Add:
                await addMedia.RunAsync();
                break;
            case MainMenuOptionsEnum.List:
                await listMedia.RunAsync();
                break;
            case MainMenuOptionsEnum.Exit:
                ContinueRunning = false;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}