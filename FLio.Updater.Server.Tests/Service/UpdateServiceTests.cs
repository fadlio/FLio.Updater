using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FLio.Updater.Server.Options;
using FLio.Updater.Server.Service;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FLio.Updater.Server.Tests.Service
{
    [TestClass]
    public class UpdateServiceTests
    {
        private ILogger<UpdateService> _logger;
        private IConfiguration _configuration;

        public UpdateServiceTests()
        {
            var provider = new ServiceCollection()
                .AddLogging(b => b.AddConsole().AddDebug())
                .BuildServiceProvider();
            _logger = provider.GetRequiredService<ILoggerFactory>().CreateLogger<UpdateService>();
            _configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string>
            {
                {"ExePath", @"C:\Users\ferna\AppData\Local\monitor\bin\tui.monitor.cli.exe"},
                {"UpdateFolderPath", @"C:\Users\ferna\Projects\tui\tui.monitor\tui.monitor.Cli\dist"}
            }).Build();
        }

        [TestMethod]
        public async Task GetLastVersionAndSourceAsyncTest()
        {
            var (lastVersion, lastVersionSource) = await UpdateService.GetLastVersionAndSourceAsync(new[]
                {
                    new UpdateSource
                    {
                        Type = "Local",
                        Uri = @"C:\Users\ferna\Downloads"
                    },
                    new UpdateSource
                    {
                        Type = "FTP",
                        Uri = "ftp.intuicao.com.br",
                        Username = "releases@files.intuicao.com.br",
                        Password = "!NT326ao",
                        ExtraPath = "tui-monitor"
                    }
                }
            );
            Assert.IsNotNull(lastVersion);
            Assert.IsTrue(lastVersionSource?.Type == "FTP");
        }
    }
}