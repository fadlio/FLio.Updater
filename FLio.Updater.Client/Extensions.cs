using FLio.Updater.Client.Ipc;
using FLio.Updater.Client.Service;
using Microsoft.Extensions.DependencyInjection;

namespace FLio.Updater.Client;

public static class Extensions
{
    public static void AddUpdateClient(
        this IServiceCollection services
    )
    {
        services.AddSingleton<UpdaterPipeClient>();
        services.AddSingleton<UpdateClientService>();
    }
}