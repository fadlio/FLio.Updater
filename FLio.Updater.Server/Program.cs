using FLio.Updater.Client.Ipc;
using FLio.Updater.Server.Ipc;
using FLio.Updater.Server.Options;
using FLio.Updater.Server.Service;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;

namespace FLio.Updater.Server;

internal static class Program
{
    private static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    private static IHostBuilder CreateHostBuilder(string[] args)
    {
        return Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context, builder) =>
            {
                builder.AddJsonEmbedded(context.HostingEnvironment.IsDevelopment()
                    ? "updater.Development.json"
                    : "updater.json");
            })
            .ConfigureServices(
                (builder, services) =>
                {
                    //Logging
                    services.AddLogging(loggingBuilder =>
                    {
                        loggingBuilder.ClearProviders();
                        loggingBuilder.AddNLog();
                    });

                    //Options
                    services.AddOptionsWithValidation<GeneralOptions>(builder);
                    services.AddOptionsWithValidation<TimersOptions>(builder);
                    services.AddOptionsWithValidation<VisualOptions>(builder);

                    //IPC
                    services.AddSingleton<IUpdaterPipeServer, UpdaterPipeServer>();

                    //Services
                    services.AddSingleton<ProcessService>();
                    services.AddSingleton<UpdateService>();
                    services.AddSingleton<PipeServerService>();

                    //Main
                    services.AddHostedService<Worker>();
                }
            );
    }
}