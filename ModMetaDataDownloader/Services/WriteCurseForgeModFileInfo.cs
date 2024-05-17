using System.Globalization;
using System.Text.RegularExpressions;
using Coravel.Invocable;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;
using ModMetaDataDownloader.Model;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace ModMetaDataDownloader.Services;

public class WriteCurseForgeModFileInfo : IInvocable
{
    private readonly InfluxDbService _influxDbService;
    private readonly ILogger<WriteCurseForgeModFileInfo> _logger;
    private readonly ModFileInfoScrapper _modFileInfoScrapper;

    public WriteCurseForgeModFileInfo(InfluxDbService influxDbService, ModFileInfoScrapper modFileInfoScrapper,
        ILogger<WriteCurseForgeModFileInfo> logger)
    {
        _influxDbService = influxDbService;
        _modFileInfoScrapper = modFileInfoScrapper;
        _logger = logger;
    }

    public async Task Invoke()
    {
        var requestTime = DateTimeOffset.UtcNow.ToUniversalTime();
        requestTime = requestTime.AddSeconds(-requestTime.Second);
        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation("[{Time}] Getting data from curse...", requestTime.ToString());

        var modFileElements = new Dictionary<string, List<FileElement>>();

        var modFileTables = await _modFileInfoScrapper.GetSiteContent();
        foreach (var (modName, fileTableHtml) in modFileTables)
        {
            var fileElements = await _modFileInfoScrapper.ParseHtml(fileTableHtml);
            modFileElements.Add(modName, fileElements);
        }
        foreach (var (modName, fileElements) in modFileElements)
        {
            await Task.Run(() =>
            {
                var bucket = _influxDbService.InfluxConfig.Buckets[modName];
                _logger.LogInformation("Writing curse download counts for '{ModName}' to bucket '{Bucket}'...", bucket.desc, bucket.name);
                fileElements.ForEach(async e =>
                {
                    _influxDbService.Write(writeApi =>
                    {
                        // TODO: FIXME: could be simplified by restricting reading of newer versions which have a uniform naming convention
                        var tokens = e.Name.Split('-');
                        var pattern = "";
                        if (tokens.Length == 4)
                        {
                            pattern = @"^(?<mcversion>[^-]+)-(?<modversion>.+)-(?<modloader>[^-]+)$";
                        }
                        if (tokens.Length == 3)
                        {
                            pattern = @"^(?<mcversion>[^-]+)-(?<modversion>.+)$";
                        }
                        var match = Regex.Match(e.Name, pattern);
                        if (match.Success)
                        {
                            var modVersion = match.Groups["modversion"].Value;
                            var modLoader = match.Groups["modloader"].Value;
                            var mcVersion = match.Groups["mcversion"].Value;
                            if (tokens.Length == 3)
                            {
                                // fallback for old versions
                                // TODO: FIXME: could be simplified by restricting reading of newer versions which have a uniform naming convention
                                modLoader = "forge";
                            }
                            var modloaders = e.ModLoader.Select(e => e.ToLower());
                            var downloadCount = new CurseForgeDownload()
                            { 
                                FileId = e.FileId, 
                                FileName = e.Name,
                                ModVersion = modVersion,
                                ModLoader = modLoader.ToLower(),
                                ModLoaders = string.Join(',', modloaders),
                                MinecraftVersion = mcVersion,
                                MinecraftVersions = string.Join(',', e.Versions),
                                Count = e.DownloadCount,
                                ReleaseType = e.ReleaseType.ToLower(),
                                ReleaseDate = e.ReleaseDate.ToUniversalTime().ToString(CultureInfo.CurrentCulture),
                                Timestamp = requestTime
                            };
                            writeApi.WriteMeasurement(downloadCount, WritePrecision.S, bucket.name,
                                _influxDbService.InfluxConfig.Org);
                        }
                    });
                });
            });
        }
    }
}