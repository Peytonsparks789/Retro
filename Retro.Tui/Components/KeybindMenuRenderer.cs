using Retro.Tui.Data.Config;
using Spectre.Console;

namespace Retro.Tui.Components;

public class KeybindMenuRenderer<T> : LayoutComponent<List<MenuOption<T>>, MenuOption<T>>
where T : Enum
{
    public override Task<MenuOption<T>> RunAsync(List<MenuOption<T>> options)
    {
        ArgumentNullException.ThrowIfNull(options);
        
        if (options.Count == 0)
            throw new ArgumentException(
                "Options cannot be null or empty.",
                nameof(options));

        if (options.Any(x => x.Key is null || x.KeyLabel is null))
            throw new ArgumentException(
                "Every option must have a key and key label.",
                nameof(options));
        
        foreach (var option in options)
        { 
            AnsiConsole.Markup(
                $"[grey]{Markup.Escape(option.KeyLabel!)}[/] " +
                $"[bold]{Markup.Escape(option.Label)}[/]  "
                );
        }
        AnsiConsole.MarkupLine("");

        while (true)
        {
            var key = Console.ReadKey(intercept: true);
            var selection = options.FirstOrDefault(x => x.Key == key.Key);

            if (selection is not null)
                return Task.FromResult(selection);
        }
    }
}

public class SelectMenuRenderer<T> : LayoutComponent<List<MenuOption<T>>, MenuOption<T>>
where T : Enum
{
    public override Task<MenuOption<T>> RunAsync(List<MenuOption<T>> options)
    {
        ArgumentNullException.ThrowIfNull(options);
        
        if (options.Count == 0)
            throw new ArgumentException(
                "Options cannot be null or empty.",
                nameof(options));
        
        var selection = AnsiConsole.Prompt(
            new SelectionPrompt<MenuOption<T>>()
                .Title("Select an option:")
                .AddChoices(options)
                .UseConverter(option => option.Label)
        );

        return Task.FromResult(selection);
    }
}