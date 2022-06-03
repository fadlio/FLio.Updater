using System;
using System.Threading.Tasks;
using FLio.Updater.Client.Ipc;
using FLio.Updater.Server.Service;
using Microsoft.Extensions.Logging;

namespace FLio.Updater.Server.Ipc;

public class UpdaterPipeServer : IUpdaterPipeServer
{
    private readonly UpdateService _updateService;
    private readonly ILogger<UpdaterPipeServer> _logger;

    public UpdaterPipeServer(ILogger<UpdaterPipeServer>logger, UpdateService updateService)
    {
        _logger = logger;
        _updateService = updateService;
    }

    public async Task<Version?> CheckForUpdate()
    {
        _logger.LogInformation("CheckForUpdate request");
        return await _updateService.GetLastVersionAsync();
    }

    public async Task Update()
    {
        _logger.LogInformation("Update request");
        await _updateService.UpdateAsync();
    }
}