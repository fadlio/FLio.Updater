using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FLio.Updater.Client.Ipc;
using FLio.Updater.Server.Ipc;
using FLio.Updater.Server.Options;
using FLio.Updater.Server.Service;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FLio.Updater.Server.Tests.Service;

[TestClass]
public class PipeServerServiceTests
{
    private readonly PipeServerService _pipeServerService;

    public PipeServerServiceTests()
    {
        var provider = new ServiceCollection()
            .AddLogging(b => b.AddConsole().AddDebug())
            .AddSingleton(new GeneralOptions
            {
                ApplicationName = "ipc-client",
                Sources = new List<UpdateSource> {new() {Type = "Local", Uri = "C:/temp/tui-client"}}
            })
            .AddSingleton<IUpdaterPipeServer, UpdaterPipeServer>()
            .AddSingleton<UpdateService>()
            .AddSingleton<PipeServerService>()
            .BuildServiceProvider();
        _pipeServerService = provider.GetRequiredService<PipeServerService>();
    }

    [TestMethod]
    public async Task ConnectAsyncTest()
    {
        await _pipeServerService.WaitForConnectionAsync();
        Console.WriteLine("Connected");
        await Task.Delay(5000);
    }
}