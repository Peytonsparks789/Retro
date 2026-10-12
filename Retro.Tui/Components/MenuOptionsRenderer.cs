using Retro.Tui.Data.Config;
using Spectre.Console;

namespace Retro.Tui.Components;

public class MenuOptionsRenderer<T>
where T : Enum
{
    public MenuOption<T> RenderAsync(List<MenuOption<T>> options)
    {
        MenuOption<T>? input = null;
        foreach (var option in options)
        { 
            AnsiConsole.Markup(
                $"[grey]{Markup.Escape(option.KeyLabel)}[/] " +
                $"[bold]{Markup.Escape(option.Label)}[/]  "
                );
        }
        AnsiConsole.MarkupLine("");

        while (input == null)
        {
            var key = Console.ReadKey(intercept: true);
            input = options.SingleOrDefault(x => x.Key == key.Key) ?? null;
        }

        return input;
    }
}