using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using FLio.Updater.Server.Options;
using FluentFTP;
using Microsoft.Extensions.Logging;

namespace FLio.Updater.Server.Service;

public class UpdateService
{
    private readonly ILogger _logger;
    private readonly List<UpdateSource> _sources;
    private readonly ProcessService _processService;

    public UpdateService(
        ILogger<UpdateService> logger,
        GeneralOptions generalOptions,
        ProcessService processService
    )
    {
        _logger = logger;
        _processService = processService;
        _sources = generalOptions.Sources;
    }

    public async Task<bool> CheckUpdateAsync()
    {
        var curVersion = _processService.GetCurrentFileVersion();
        var lastVersion = await GetLastVersionAsync();

        return curVersion?.CompareTo(lastVersion) < 0;
    }

    public async Task UpdateAsync()
    {
        _logger.LogInformation("Updating");

        var curVersion = _processService.GetCurrentFileVersion();
        var (lastVersion, lastVersionSource) = await GetLastVersionAndSourceAsync(_sources);

        if (curVersion?.CompareTo(lastVersion) >= 0)
        {
            _logger.LogInformation("Last version already installed");
            return;
        }

        // Update process
        if (Directory.Exists(PathService.BinFolder))
            Directory.Delete(PathService.BinFolder, true);
        await CopyItemByVersionFromSource("temp.zip", lastVersion, lastVersionSource);
        await Task.Run(() =>
            ZipFile.ExtractToDirectory("temp.zip", PathService.BinFolder)
        );

        if (curVersion == null)
        {
            _logger.LogInformation("Initial installation");
        }
        else if (curVersion.CompareTo(lastVersion) < 0)
        {
            _logger.LogInformation(
                "Updated from {Prev} to {Last}",
                curVersion.ToString(3), lastVersion.ToString(3)
            );
        }
    }

    public async Task<Version> GetLastVersionAsync()
    {
        return (await GetLastVersionAndSourceAsync(_sources)).Item1;
    }

    private static async Task CopyItemByVersionFromSource(string destination, Version version, UpdateSource source)
    {
        await using Stream tempFile = File.Create(Path.Join(PathService.BaseFolder, destination));
        switch (source.Type)
        {
            case "FTP":
                var client = new FtpClient(source.Uri, source.Username, source.Password);
                await client.ConnectAsync();
                await client.DownloadAsync(tempFile, Path.Join(source.ExtraPath, version.ToZipFileName()));
                client.Dispose();
                break;
            default:
            {
                await using Stream from = File.Open(Path.Join(source.Uri, version.ToZipFileName()), FileMode.Open);
                await from.CopyToAsync(tempFile);
                break;
            }
        }
    }

    private static async Task<Version?> GetFtpSourceLastVersion(UpdateSource source)
    {
        var client = new FtpClient(source.Uri, source.Username, source.Password);
        var folderPath = Path.Join("/", source.ExtraPath);
        var folderExists = await client.DirectoryExistsAsync(folderPath);
        if (!folderExists) return null;

        var items = new List<FtpListItem>(await client.GetListingAsync(folderPath));
        client.Dispose();

        return items
            .Where(i => i.Name.EndsWith(".zip"))
            .Select(i =>
                {
                    try
                    {
                        return new Version(i.Name[..^4]);
                    }
                    catch (Exception)
                    {
                        return new Version();
                    }
                }
            )
            .Max();
    }

    private static Version? GetLocalSourceLastVersion(UpdateSource source)
    {
        var folder = new DirectoryInfo(source.Uri);
        return folder.EnumerateFiles("*.zip")
            .Select(i =>
                {
                    try
                    {
                        return new Version(i.Name[..^4]);
                    }
                    catch (Exception)
                    {
                        return new Version();
                    }
                }
            )
            .Max();
    }

    public static async Task<Tuple<Version, UpdateSource>> GetLastVersionAndSourceAsync(
        IEnumerable<UpdateSource> sources)
    {
        UpdateSource? overallLastSource = null;
        var overallLastVersion = new Version();
        
        foreach (var source in sources)
        {
            var sourceLastVersion = source.Type switch
            {
                "FTP" => await GetFtpSourceLastVersion(source),
                "Local" => GetLocalSourceLastVersion(source),
                _ => null
            };

            if (sourceLastVersion == null || overallLastVersion.CompareTo(sourceLastVersion) >= 0) continue;
            
            overallLastVersion = sourceLastVersion;
            overallLastSource = source;
        }

        if (overallLastSource == null || overallLastVersion == null)
            throw new FileNotFoundException("Could not find any update files.");

        return new Tuple<Version, UpdateSource>(overallLastVersion, overallLastSource);
    }
}