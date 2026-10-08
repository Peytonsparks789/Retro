using Microsoft.Extensions.DependencyInjection;
using Retro.Infrastructure.Database;
using Retro.Infrastructure.Database.DependencyInjection;
using Retro.Tui.Components;

var services = new ServiceCollection()
    .AddSqliteDb()
    .AddMediaRepository()
    
    // TUI Components
    .AddTransient<MainMenu>();

var provider = services.BuildServiceProvider();

provider
    .GetRequiredService<DatabaseInitializer>()
    .Initialize();

var app = provider.GetRequiredService<MainMenu>();

await app.RunAsync();