using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FLio.Updater.Client.Ipc;

public class UpdaterPipeClient : IUpdaterPipeClient
{
    private readonly ILogger<UpdaterPipeClient> _logger;
    private readonly IHostApplicationLifetime _hostApplicationLifetime;
    public event EventHandler<Version>? OnNewVersionNotified;

    public UpdaterPipeClient(ILogger<UpdaterPipeClient> logger, IHostApplicationLifetime hostApplicationLifetime)
    {
        _logger = logger;
        _hostApplicationLifetime = hostApplicationLifetime;
    }
    public void Shutdown()
    {
        _logger.LogInformation("Shutdown request");
        Task.Run(() =>
        {
            _hostApplicationLifetime.StopApplication();
        });
    }

    public void NotifyNewVersion(Version version)
    {
        _logger.LogInformation("NotifyNewVersion request");
        OnNewVersionNotified?.Invoke(this, version);
    }
}