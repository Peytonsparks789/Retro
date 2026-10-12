namespace Retro.Tui.Data.Config;

public record MenuOption<T>(
    T Operation,
    string Label,
    ConsoleKey Key,
    string KeyLabel
    );