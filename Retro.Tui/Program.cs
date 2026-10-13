using Microsoft.Extensions.DependencyInjection;
using Retro.Infrastructure.Database;
using Retro.Infrastructure.Database.DependencyInjection;
using Retro.Tui.Components;
using Retro.Tui.Data.Enums;

var services = new ServiceCollection()
    .AddSqliteDb()
    .AddMediaRepository()
    
    // TUI Components
    .AddTransient<MainMenu>()
    .AddTransient<AddMedia>()
    .AddTransient<ListMedia>()
    .AddTransient<ViewMedia>()
    
    // TUI Menu Options
    .AddSingleton<KeybindMenuRenderer<MainMenuOptionsEnum>>()
    .AddSingleton<KeybindMenuRenderer<MediaListOptionsEnum>>()
    .AddSingleton<KeybindMenuRenderer<MediaViewOptionsEnum>>()
    ;

var provider = services.BuildServiceProvider();

provider
    .GetRequiredService<DatabaseInitializer>()
    .Initialize();

var app = provider.GetRequiredService<MainMenu>();

await app.RunAsync();