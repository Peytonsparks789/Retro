using Retro.Infrastructure.Database;
using Retro.Tui.Data.Config;
using Retro.Tui.Data.Enums;
using Spectre.Console;

namespace Retro.Tui.Components;

public class ListMedia(IMediaRepository repository)
{
    private readonly List<MenuOption<MediaListOption>> _options = [
        new(MediaListOption.Next, "Next page"),
        new(MediaListOption.Previous, "Previous page"),
        new(MediaListOption.Select, "Select media by ID"),
        new(MediaListOption.Search, "Search"),
        new(MediaListOption.Back, "Back")
    ];
    
    public async Task ListMediaEntriesAsync()
    {
        while (true)
        {
            AnsiConsole.Clear();

            RetroHeader.GenerateHeader();
            
            var data = await repository.GetMediaSummaryAsync(
                search: null,
                pageSize: 10);
            
            if (data.Count == 0)
            {
                AnsiConsole.MarkupLine("[red]No media entries found.[/]");
                AnsiConsole.Prompt(
                    new TextPrompt<string>("Press any key to go back to the main menu.\"").AllowEmpty()
                );
                return;
            }

            var table = new Table();

            table.AddColumn("Id");
            table.AddColumn("Title");
            table.AddColumn("Medium");
            table.AddColumn("Rating");
            
            foreach (var row in data)
            {
                table.AddRow(
                    row.Id.ToString(), 
                    row.Title, 
                    row.MediumType.ToString(), 
                    row.Stars.ToString("F1"));
            }
            
            AnsiConsole.Write(table);
            
            var input = AnsiConsole.Prompt(
                new SelectionPrompt<MenuOption<MediaListOption>>()
                    .Title("What would you like to do?")
                    .AddChoices(_options)
                    .UseConverter(option => option.Label));

            switch (input.Operation)
            {
                case MediaListOption.Next:
                    break;
                case MediaListOption.Previous:
                    break;
                case MediaListOption.Select:
                    // Select media by ID
                    break;
                case MediaListOption.Search:
                    // Search
                    break;
                case MediaListOption.Back:
                    return;
            }
        }
    }
}