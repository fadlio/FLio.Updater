using FLio.Updater.Client.Ipc;
using Microsoft.Extensions.Logging;
using PipeMethodCalls;
using PipeMethodCalls.MessagePack;

namespace FLio.Updater.Client.Service;

public class UpdateClientService
{
    private readonly ILogger<UpdateClientService> _logger;
    private readonly PipeClientWithCallback<IUpdaterPipeServer, IUpdaterPipeClient> _pipeClient;

    public event EventHandler<Version>? OnNewVersionNotified;

    public UpdateClientService(ILogger<UpdateClientService> logger, UpdaterPipeClient updaterPipeClient)
    {
        _logger = logger;
        string pipeName;
        try
        {
            pipeName = File.ReadAllText("update-pipe");
        }
        catch (FileNotFoundException)
        {
            throw new FileNotFoundException($"No update-pipe file found at {Directory.GetCurrentDirectory()}");
        }

        updaterPipeClient.OnNewVersionNotified += OnNewVersionNotified;

        _pipeClient = new PipeClientWithCallback<IUpdaterPipeServer, IUpdaterPipeClient>(
            new MessagePackPipeSerializer(),
            pipeName,
            () => updaterPipeClient
        );
    }

    public async Task ConnectAsync(CancellationToken? cancellationToken = null)
    {
        var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var timeoutToken = tokenSource.Token;
        await _pipeClient.ConnectAsync(cancellationToken ?? timeoutToken);
        if (timeoutToken.IsCancellationRequested)
        {
            throw new TimeoutException("Could not connect to the server");
        }
        tokenSource.Dispose();

        _logger.LogInformation("Pipe connected");
    }

    public async Task<Version?> CheckForUpdateAsync()
    {
        return await _pipeClient.InvokeAsync(s => s.CheckForUpdate());
    }

    public async Task UpdateAsync()
    {
        await _pipeClient.InvokeAsync(s => s.Update());
    }
}