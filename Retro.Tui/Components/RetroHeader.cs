using Spectre.Console;

namespace Retro.Tui.Components;

public static class RetroHeader
{
    public static void GenerateHeader()
    {
        AnsiConsole.MarkupLine("");
        AnsiConsole.MarkupLine(@"[green] ░       ░░░        ░░        ░░       ░░░░      ░░ [/]");
        AnsiConsole.MarkupLine(@"[green] ▒  ▒▒▒▒  ▒▒  ▒▒▒▒▒▒▒▒▒▒▒  ▒▒▒▒▒  ▒▒▒▒  ▒▒  ▒▒▒▒  ▒ [/]");
        AnsiConsole.MarkupLine(@"[green] ▓       ▓▓▓      ▓▓▓▓▓▓▓  ▓▓▓▓▓       ▓▓▓  ▓▓▓▓  ▓ [/]");
        AnsiConsole.MarkupLine(@"[green] █  ███  ███  ███████████  █████  ███  ███  ████  █ [/]");
        AnsiConsole.MarkupLine(@"[green] █  ████  ██        █████  █████  ████  ███      ██ [/]");
        AnsiConsole.MarkupLine("");
    }
}