using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FLio.Updater.Server.Options;
using Microsoft.Extensions.Logging;

namespace FLio.Updater.Server.Service;

public class ProcessService
{
    private readonly Process _process;
    private ILogger<ProcessService> _logger;

    public bool FileExists => File.Exists(_process.StartInfo.FileName);

    public bool IsAlive
    {
        get
        {
            _process.Refresh();
            return !_process.HasExited;
        }
    }

    public ProcessService(
        ILogger<ProcessService> logger,
        GeneralOptions generalOptions,
        VisualOptions visualOptions
    )
    {
        _logger = logger;
        var processPath = Path.Join(PathService.BinFolder, $"{generalOptions.ApplicationName}.exe");
        _process = new Process();
        var startInfo = new ProcessStartInfo
        {
            UseShellExecute = !visualOptions.Hidden,
            CreateNoWindow = visualOptions.Hidden,
            WorkingDirectory = PathService.BinFolder,
            WindowStyle = visualOptions.Hidden
                ? ProcessWindowStyle.Hidden
                : ProcessWindowStyle.Normal,
            FileName = processPath,
            RedirectStandardOutput = visualOptions.Hidden
        };
        _process.StartInfo = startInfo;
    }

    public void ClosePreviousInstances()
    {
        var processName = Path.GetFileNameWithoutExtension(GetExactPathName(_process.StartInfo.FileName));
        foreach (var process in Process.GetProcessesByName(processName))
            process.Kill(true);
    }

    public void Start()
    {
        _logger.LogInformation(
            "Starting process. Version: {Version}",
            GetCurrentFileVersion()
        );
        _process.Start();
        if (_process.StartInfo.RedirectStandardOutput)
            _process.BeginOutputReadLine();
    }

    public Version? GetCurrentFileVersion()
    {
        if (!File.Exists(_process.StartInfo.FileName)) return null;
        var versionInfo = FileVersionInfo.GetVersionInfo(_process.StartInfo.FileName);
        return versionInfo.FileVersion != null ? new Version(versionInfo.FileVersion) : null;
    }

    public async Task WaitForExitAsync()
    {
        _logger.LogTrace("Waiting for process to exit...");
        while (!_process.HasExited)
        {
            _process.Refresh();
            await Task.Delay(100, CancellationToken.None);
        }
    }

    private static string GetExactPathName(string pathName)
    {
        if (!(File.Exists(pathName) || Directory.Exists(pathName)))
            return pathName;

        var directoryInfo = new DirectoryInfo(pathName);

        if (directoryInfo.Parent != null)
            return Path.Combine(
                GetExactPathName(directoryInfo.Parent.FullName),
                directoryInfo.Parent.GetFileSystemInfos(directoryInfo.Name)[0].Name
            );

        return directoryInfo.Name.ToUpper();
    }
}