using Retro.Core.Data;
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

    private bool _continueListing = true;
    private long _cursor;
    private bool _orderAscending = true;
    private bool _hasPreviousPage = false;
    private bool _hasNextPage = false;
    
    public async Task ListMediaEntriesAsync()
    {
        _cursor = 0;
        _orderAscending = true;
        
        while (_continueListing)
        {
            AnsiConsole.Clear();

            RetroHeader.GenerateHeader();
            
            var data = await GetMediaDataAsync();

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
            
            DeterminePageAvailability(data);
            foreach (var row in ResolveMediaList(data))
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

            HandleUserSelection(input, data);
        }
    }
    
    private async Task<List<MediaSummary>> GetMediaDataAsync()
        => await repository.GetMediaSummaryAsync(
            search: null,
            pageSize: 10,
            lastId: _cursor,
            sortAscending: _orderAscending);
    
    private List<MediaSummary> ResolveMediaList(List<MediaSummary> data)
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
    
    private void HandleUserSelection(MenuOption<MediaListOption> input, List<MediaSummary> data)
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
                // Select media by ID
                break;
            case MediaListOption.Search:
                // Search
                break;
            case MediaListOption.Back:
                _continueListing = false;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}