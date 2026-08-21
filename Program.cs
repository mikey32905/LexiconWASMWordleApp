//TODO:
// 1. Add in branding logos
// 2. Update landing page so this can be added to portfolio
// 3. Add in a "How to Play" page
// 4. Redo how tiles are colored for correct letters. (currently tile borders are highlighted. whole tile needs to change color.
// 5. Add in a "Daily Challenge" mode that is the same for all players each day. (like the original Wordle game)
// 6. Add in a "Freeplay" mode that allows players to play as many games as they want with random words.
// 7. Add in a "Hard Mode" that requires players to use correct letters in subsequent guesses.



using LexiconWASMWordleApp;
using LexiconWASMWordleApp.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
// Register the service with the DI container
builder.Services.AddScoped<IGameService, GameService>();
builder.Services.AddScoped<IStatsService, StatsService>();

await builder.Build().RunAsync();
