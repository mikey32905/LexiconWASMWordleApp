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
