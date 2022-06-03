using FLio.Updater.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FLio.Updater.TestClient;

internal static class Program
{
    private static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    private static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                // UpdateClientService
                services.AddUpdateClient();
                
                // Worker
                services.AddHostedService<Worker>();
            });
}