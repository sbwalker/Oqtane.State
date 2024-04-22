using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Oqtane.State.Client.Models;

namespace Oqtane.State.Client
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);

            builder.Services.AddScoped<SiteState>();

            await builder.Build().RunAsync();
        }
    }
}
