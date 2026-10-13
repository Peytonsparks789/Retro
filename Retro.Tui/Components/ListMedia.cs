using Retro.Core.Data;
using Retro.Infrastructure.Database;
using Retro.Tui.Data.Config;
using Retro.Tui.Data.Enums;
using Spectre.Console;

namespace Retro.Tui.Components;

public class ListMedia(
    KeybindMenuRenderer<MediaListOptionsEnum> options,
    IMediaRepository repository,
    LayoutComponent<ViewMediaOptions> viewMedia) : LayoutComponent
{
    private readonly List<MenuOption<MediaListOptionsEnum>> _options = [
        new(MediaListOptionsEnum.Next, "Next page", ConsoleKey.RightArrow, "→"),
        new(MediaListOptionsEnum.Previous, "Previous page", ConsoleKey.LeftArrow, "←"),
        new(MediaListOptionsEnum.Select, "Select media by ID", ConsoleKey.Enter, "Enter"),
        new(MediaListOptionsEnum.Search, "Search", ConsoleKey.S, "S"),
        new(MediaListOptionsEnum.Back, "Back", ConsoleKey.Escape, "Esc")
    ];

    private long _cursor;
    private bool _orderAscending = true;
    private bool _hasPreviousPage;
    private bool _hasNextPage;
    private string? _search;
    
    public override async Task RunAsync()
    {
        try
        {
            ResetDefaults();
            while (ContinueRunning)
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
        
                var input = await options.RunAsync(_options);
                await HandleUserSelection(input, data);
            }
        }
        catch (Exception e)
        {
            AnsiConsole.MarkupLine($"[red]An error occurred while listing media entries: {Markup.Escape(e.Message)}[/]");
            AnsiConsole.Prompt(new TextPrompt<string>("[red]Press any key to continue...[/]").AllowEmpty());
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
    
    private async Task HandleUserSelection(MenuOption<MediaListOptionsEnum> input, List<MediaSummary> data)
    {
        switch (input.Operation)
        {
            case MediaListOptionsEnum.Next:
                if (_hasNextPage)
                {
                    _hasPreviousPage = true;
                    _orderAscending = true;
                    _cursor = ResolveFinalListId(data);
                }
                break;
            case MediaListOptionsEnum.Previous:
                if (_hasPreviousPage)
                {
                    _hasNextPage = true;
                    _orderAscending = false;
                    _cursor = ResolveFinalListId(data);
                }
                break;
            case MediaListOptionsEnum.Select:
                var id = AnsiConsole.Prompt(
                    new TextPrompt<long>("[green]Enter media ID to view details:[/]").Validate(id =>
                    {
                        if (id < 0)
                        {
                            return ValidationResult.Error("[red]Please enter a valid positive ID.[/]");
                        }
                        return ValidationResult.Success();
                    }));
                await viewMedia.RunAsync(new ViewMediaOptions { Id = id });
                break;
            case MediaListOptionsEnum.Search:
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
            case MediaListOptionsEnum.Back:
                ResetDefaults();
                ContinueRunning = false;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    protected override void ResetDefaults()
    {
        ContinueRunning = true;
        _cursor = 0;
        _orderAscending = true;
        _hasPreviousPage = false;
        _hasNextPage = false;
        _search = null;
    }
}