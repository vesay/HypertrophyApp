using System.Globalization;
using HypertrophyApp.Web;
using HypertrophyApp.Web.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;

// The UI is in English, so format numbers and dates consistently regardless of the visitor's OS locale.
CultureInfo.DefaultThreadCurrentCulture = CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("en-US");

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// The client is served by HypertrophyApp.Api, so the API lives on the same origin.
builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped<HypertrophyAppApiClient>();
builder.Services.AddScoped<LocalSettings>();
builder.Services.AddScoped<AppState>();
builder.Services.AddScoped<ThemeState>();
builder.Services.AddMudServices();

await builder.Build().RunAsync();
