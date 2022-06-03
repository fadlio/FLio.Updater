using FLio.Updater.Client.Service;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FLio.Updater.TestClient;

public class Worker : IHostedService
{
    private readonly UpdateClientService _updateService;
    private readonly ILogger<Worker> _logger;

    public Worker(UpdateClientService updateClientService, ILogger<Worker> logger)
    {
        _logger = logger;
        _updateService = updateClientService;
        _updateService.OnNewVersionNotified += (sender, version) =>
        {
            Console.WriteLine(version);
        };
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _updateService.ConnectAsync();
        var lastVersion = await _updateService.CheckForUpdateAsync();
        _logger.LogInformation("{Version}",lastVersion);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await Task.Delay(1000, cancellationToken);
    }
}