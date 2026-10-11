using Retro.Core.Data;
using Retro.Infrastructure.Database;
using Retro.Tui.Data.Config;
using Retro.Tui.Data.Enums;
using Spectre.Console;

namespace Retro.Tui.Components;

public class ListMedia(
    IMediaRepository repository,
    ViewMedia viewMedia)
{
    private readonly List<MenuOption<MediaListOption>> _options = [
        new(MediaListOption.Next, "Next page"),
        new(MediaListOption.Previous, "Previous page"),
        new(MediaListOption.Select, "Select media by ID"),
        new(MediaListOption.Search, "Search"),
        new(MediaListOption.Back, "Back")
    ];

    private bool _continueListing = true;
    private long _cursor;
    private bool _orderAscending = true;
    private bool _hasPreviousPage;
    private bool _hasNextPage;
    private string? _search;
    
    public async Task ListMediaEntriesAsync()
    {
        try
        {
            ResetDefaults();
            while (_continueListing)
            {
                await Run();
            }
        }
        catch (Exception e)
        {
            AnsiConsole.MarkupLine($"[red]An error occurred while listing media entries: {Markup.Escape(e.Message)}[/]");
            AnsiConsole.Prompt(new TextPrompt<string>("[red]Press any key to continue...[/]").AllowEmpty());
        }
    }
    
    private async Task Run()
    {
        AnsiConsole.Clear();

        RetroHeader.GenerateHeader();
                
        var data = await GetMediaDataAsync();

        if (_search is not null)
        {
            AnsiConsole.MarkupLine($"[green]Search: [/]{Markup.Escape(_search)}");
        }

        if (data.Count > 0)
        {
            var table = new Table();
            
            table.AddColumn("Id");
            table.AddColumn("Title");
            table.AddColumn("Medium");
            table.AddColumn("Rating");
            
            DeterminePageAvailability(data);
            foreach (var row in ResolveMediaList(data))
            {
                table.AddRow(
                    row.Id.ToString(), 
                    Markup.Escape(row.Title), 
                    Markup.Escape(row.MediumType.ToString()), 
                    row.Stars.ToString("F1"));
            }
            
            AnsiConsole.Write(table);
        }
        else
        {
            AnsiConsole.MarkupLine("[red]No media entries found for the given search.[/]");
        }
                
        AnsiConsole.MarkupLine(
            "[grey]←[/] [bold]Previous[/]    " +
            "[grey]→[/] [bold]Next[/]    " +
            "[grey]Enter[/] [bold]Select[/]    " +
            "[grey]S[/] [bold]Search[/]    " +
            "[grey]Esc[/] [bold]Back[/]"
            );
        var key = Console.ReadKey(intercept: true);

        var input = key.Key switch
        {
            ConsoleKey.LeftArrow => _options.Single(x => x.Operation == MediaListOption.Previous),
                ConsoleKey.RightArrow => _options.Single(x => x.Operation == MediaListOption.Next),
                ConsoleKey.Enter => _options.Single(x => x.Operation == MediaListOption.Select),
                ConsoleKey.S => _options.Single(x => x.Operation == MediaListOption.Search),
                ConsoleKey.Escape => _options.Single(x => x.Operation == MediaListOption.Back),
                _ => null
        };

        if (input is not null)
        {
            await HandleUserSelection(input, data);
        }
    }
    
    private async Task<List<MediaSummary>> GetMediaDataAsync()
        => await repository.GetMediaSummaryAsync(
            search: _search,
            pageSize: 10,
            lastId: _cursor,
            sortAscending: _orderAscending);
    
    private static List<MediaSummary> ResolveMediaList(List<MediaSummary> data)
    {
        List<MediaSummary> items = [.. data.Take(10)];
        return [.. items.OrderBy(x => x.Id)];
    }
    
    private void DeterminePageAvailability(List<MediaSummary> data)
    {
        if (_orderAscending)
        {
            _hasNextPage = data.Count > 10;
        }
        else
        {
            _hasPreviousPage = data.Count > 10;
        }
    }

    private long ResolveFinalListId(List<MediaSummary> data)
    {
        var items = ResolveMediaList(data);
        return _orderAscending ? items.Last().Id : items.First().Id;
    }
    
    private async Task HandleUserSelection(MenuOption<MediaListOption> input, List<MediaSummary> data)
    {
        switch (input.Operation)
        {
            case MediaListOption.Next:
                if (_hasNextPage)
                {
                    _hasPreviousPage = true;
                    _orderAscending = true;
                    _cursor = ResolveFinalListId(data);
                }
                break;
            case MediaListOption.Previous:
                if (_hasPreviousPage)
                {
                    _hasNextPage = true;
                    _orderAscending = false;
                    _cursor = ResolveFinalListId(data);
                }
                break;
            case MediaListOption.Select:
                var id = AnsiConsole.Prompt(
                    new TextPrompt<long>("[green]Enter media ID to view details:[/]").Validate(id =>
                    {
                        if (id < 0)
                        {
                            return ValidationResult.Error("[red]Please enter a valid positive ID.[/]");
                        }
                        return ValidationResult.Success();
                    }));
                await viewMedia.ViewMediaEntryAsync(id);
                break;
            case MediaListOption.Search:
                _search = AnsiConsole.Prompt(
                    new TextPrompt<string>("[green]Search [grey](leave empty to clear)[/]: [/]").AllowEmpty());
                if (string.IsNullOrWhiteSpace(_search))
                {
                    _search = null;
                }
                _cursor = 0;
                _orderAscending = true;
                _hasNextPage = false;
                _hasPreviousPage = false;
                break;
            case MediaListOption.Back:
                ResetDefaults();
                _continueListing = false;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void ResetDefaults()
    {
        _continueListing = true;
        _cursor = 0;
        _orderAscending = true;
        _hasPreviousPage = false;
        _hasNextPage = false;
        _search = null;
    }
}