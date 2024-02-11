using CruderSimple.Blazor.Example.Sf;
using CruderSimple.Blazor.Sf.Extensions;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");
builder.Logging.AddFilter((category, level) =>
{
    return !(category?.Contains("System.Net.Http.HttpClient") == true);
});
builder.Logging.SetMinimumLevel(LogLevel.Debug);

builder.Services.AddOptions();
builder.Services
    .AddCruderSimpleBlazorSf(builder.Configuration);
// TODO: auto import

builder.Services.AddOdontoManagementClient();
await builder
    .Build()
    .UseCruderSimpleBlazor()
    .RunAsync();
