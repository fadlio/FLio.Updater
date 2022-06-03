using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FLio.Updater.Client.Ipc;
using Microsoft.Extensions.Logging;
using PipeMethodCalls;
using PipeMethodCalls.MessagePack;

namespace FLio.Updater.Server.Service
{
    public class PipeServerService
    {
        private readonly ILogger<PipeServerService> _logger;
        private readonly Guid _pipeName;
        private readonly IUpdaterPipeServer _updaterPipeServer;

        private PipeServerWithCallback<IUpdaterPipeClient, IUpdaterPipeServer> _pipeServer;

        public PipeServerService(ILogger<PipeServerService> logger, IUpdaterPipeServer updaterPipeServer)
        {
            _logger = logger;
            _updaterPipeServer = updaterPipeServer;
            _pipeName = Guid.NewGuid();
            _pipeServer = GeneratePipeServer();
        }

        public void GeneratePipeFile(string path)
        {
            using var file = File.CreateText(Path.Join(path, "update-pipe"));
            file.Write(_pipeName.ToString());
        }

        public async Task WaitForConnectionAsync(CancellationToken? cancellationToken = null)
        {
            if (_pipeServer.State != PipeState.NotOpened)
            {
                _pipeServer.Dispose();
                _pipeServer = GeneratePipeServer();
            }

            _logger.LogInformation("Waiting for pipe connection");
            var timeoutCts = new CancellationTokenSource();
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(30));
            await _pipeServer.WaitForConnectionAsync(cancellationToken ?? timeoutCts.Token);
            if (timeoutCts.IsCancellationRequested)
            {
                throw new TimeoutException("Client not connected");
            }

            _logger.LogInformation("Pipe connected");
        }

        public async Task NotifyNewVersion(Version version)
        {
            if (_pipeServer.State != PipeState.Connected) return;
            _logger.LogDebug("NotifyNewVersion invoked");
            await _pipeServer.InvokeAsync(c => c.NotifyNewVersion(version));
        }

        public async Task ShutdownClientAsync()
        {
            if (_pipeServer.State != PipeState.Connected) return;
            _logger.LogDebug("Shutdown invoked");
            await _pipeServer.InvokeAsync(c => c.Shutdown());
        }

        private PipeServerWithCallback<IUpdaterPipeClient, IUpdaterPipeServer> GeneratePipeServer()
        {
            return new PipeServerWithCallback<IUpdaterPipeClient, IUpdaterPipeServer>(new MessagePackPipeSerializer(),
                _pipeName.ToString(),
                () => _updaterPipeServer);
        }
    }
}