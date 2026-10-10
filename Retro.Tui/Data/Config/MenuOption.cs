namespace Retro.Tui.Data.Config;

public record MenuOption<T>(
    T Operation,
    string Label
    );