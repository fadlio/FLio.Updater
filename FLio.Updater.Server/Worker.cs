using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using FLio.Updater.Server.Options;
using FLio.Updater.Server.Service;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Timer = System.Timers.Timer;

namespace FLio.Updater.Server;

public class Worker : IHostedService
{
    private readonly ILogger _logger;

    private readonly GeneralOptions _generalOptions;

    private readonly Timer? _aliveTimer;
    private readonly Timer _updateTimer;
    private readonly ProcessService _processService;
    private readonly UpdateService _updateService;
    private readonly PipeServerService _pipeServerService;

    public Worker(
        GeneralOptions generalOptions,
        TimersOptions timersOptions,
        ProcessService processService,
        UpdateService updateService,
        PipeServerService pipeServerService,
        ILogger<Worker> logger
    )
    {
        _logger = logger;
        _processService = processService;
        _updateService = updateService;
        _pipeServerService = pipeServerService;

        _generalOptions = generalOptions;

        if (timersOptions.AutoReopen != null)
        {
            _logger.LogInformation("AutoReopen enabled: {S}s", timersOptions.AutoReopen);
            _aliveTimer = new Timer((double) (1000 * timersOptions.AutoReopen)) {AutoReset = true};
            _aliveTimer.Elapsed += AliveTimerOnElapsed;
        }

        _logger.LogInformation("CheckForUpdates: {S}s", timersOptions.Update);
        _updateTimer = new Timer(1000 * timersOptions.Update) {AutoReset = true};
        _updateTimer.Elapsed += UpdateTimerOnElapsed;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(PathService.BinLogsFolder);

        _processService.ClosePreviousInstances();

        if (!_processService.FileExists || _generalOptions.EnforceLastVersion)
        {
            _logger.LogInformation("Updating before starting process");
            await _updateService.UpdateAsync();
        }

        await StartProcessAsync();

        _aliveTimer?.Start();
        _updateTimer.Start();
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _aliveTimer?.Stop();
        _updateTimer.Stop();

        await _pipeServerService.ShutdownClientAsync();
        await _processService.WaitForExitAsync();
    }

    private async Task StartProcessAsync()
    {
        _pipeServerService.GeneratePipeFile(PathService.BinFolder);
        _processService.Start();
        await _pipeServerService.WaitForConnectionAsync();
    }

    private async void UpdateTimerOnElapsed(object? sender, ElapsedEventArgs e)
    {
        _logger.LogDebug("Checking if new version");
        
        if (!await _updateService.CheckUpdateAsync()) return;
        
        _logger.LogInformation("New version found");
        await _pipeServerService.ShutdownClientAsync();
        await _processService.WaitForExitAsync();
        await _updateService.UpdateAsync();
    }

    private async void AliveTimerOnElapsed(object? sender, ElapsedEventArgs e)
    {
        _logger.LogDebug("Checking if process is running");
        if (_processService.IsAlive)
            return;
        _logger.LogInformation("Relaunching process");
        await StartProcessAsync();
    }
}