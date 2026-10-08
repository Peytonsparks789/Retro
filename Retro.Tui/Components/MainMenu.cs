using Retro.Tui.Data.Config;
using Spectre.Console;
using Retro.Tui.Data.Enums;

namespace Retro.Tui.Components;

public class MainMenu
{
    private readonly List<MenuOption> _options = [
        new(MainMenuOption.Exit, "Exit")
    ];
    public async Task RunAsync()
    {
        while (true)
        {
            AnsiConsole.Clear();

            RetroHeader.GenerateHeader();
            
            var selection = AnsiConsole.Prompt(
                new SelectionPrompt<MenuOption>()
                    .Title("Welcome to Retro, what would you like to do?")
                    .AddChoices(_options)
                    .UseConverter(option => option.Label));

            switch (selection.Operation)
            {
                case MainMenuOption.Exit:
                    return;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}