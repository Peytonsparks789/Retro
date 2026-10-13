using Retro.Tui.Data.Config;
using Spectre.Console;

namespace Retro.Tui.Components;

public class KeybindMenuRenderer<T> : LayoutComponent<List<MenuOption<T>>, MenuOption<T>>
where T : Enum
{
    public override Task<MenuOption<T>> RunAsync(List<MenuOption<T>> options)
    {
        ArgumentNullException.ThrowIfNull(options);
        
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
            input = options.FirstOrDefault(x => x.Key == key.Key) ?? null;
        }

        return Task.FromResult(input);
    }
}