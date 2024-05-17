using System.Globalization;
using System.Text.RegularExpressions;
using Coravel.Invocable;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;
using ModMetaDataDownloader.Model;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace ModMetaDataDownloader.Services;

public class WriteModrinthModFileInfo : IInvocable
{
    private readonly InfluxDbService _influxDbService;
    private readonly ILogger<WriteModrinthModFileInfo> _logger;
    private readonly ModrinthApiUtil _modrinthApi;
    private readonly ModrinthConfig _modrinthConfig;

    public WriteModrinthModFileInfo(IConfiguration configuration, InfluxDbService influxDbService, 
        ModrinthApiUtil modrinthApi,
        ILogger<WriteModrinthModFileInfo> logger)
    {
        _influxDbService = influxDbService;
        _modrinthApi = modrinthApi;
        _modrinthConfig =  _modrinthConfig = configuration.GetSection("ModrinthApi").Get<ModrinthConfig>();
        _logger = logger;
    }

    public async Task Invoke()
    {
        var requestTime = DateTimeOffset.UtcNow.ToUniversalTime();
        requestTime = requestTime.AddSeconds(-requestTime.Second);
        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation("[{Time}] Getting data from modrinth...", requestTime.ToString());

        foreach (var mod in _modrinthConfig.Mods.Values)
        {
            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation(
                    "[{Time}] Getting mod download stats for '{ModName}' (with Id={ModId}) from modrinth...",
                    requestTime.ToString(), mod.Slug, mod.Id);
            var modFileInfos = await _modrinthApi.GetModFileInfo(mod);
            var bucket = _influxDbService.InfluxConfig.Buckets[mod.Slug];
            _logger.LogInformation("Writing modrinth download counts for '{modName}' to bucket '{bucket}'...", bucket.desc, bucket.name);
            modFileInfos.ForEach(mfi =>
            {
                var bucket = _influxDbService.InfluxConfig.Buckets[mod.Slug];
                _influxDbService.Write(writeApi =>
                {
                    // TODO: FIXME: could be simplified by restricting reading of newer versions which have a uniform naming convention
                    var tokens = mfi.name.Split('-');
                    var pattern = "";
                    if (tokens.Length == 4)
                    {
                        pattern = @"^(?<mcversion>[^-]+)-(?<modversion>.+)-(?<modloader>[^-]+)$";
                    }

                    if (tokens.Length == 3)
                    {
                        pattern = @"^(?<mcversion>[^-]+)-(?<modversion>.+)$";
                    }

                    var match = Regex.Match(mfi.name, pattern);
                    if (match.Success)
                    {
                        var modVersion = match.Groups["modversion"].Value;
                        var modLoader = match.Groups["modloader"].Value;
                        var mcVersion = match.Groups["mcversion"].Value;
                        if (tokens.Length == 3)
                        {
                            // fallback for old versions
                            modLoader = "forge";
                        }
                        
                        var modloaders = mfi.loaders.Select(e => e.ToLower());
                        var downloadCount = new ModrinthDownload()
                        {
                            FileId = mfi.id,
                            FileName = mfi.name,
                            ModVersion = modVersion, // mfi.version_number, TODO: version number could only be used for newer versions, since older dont have a uniform naming
                            ModLoader = modLoader.ToLower(),
                            ModLoaders = string.Join(',', modloaders),
                            MinecraftVersion = mcVersion,
                            MinecraftVersions = string.Join(',', mfi.game_versions),
                            Count = mfi.downloads,
                            ReleaseType = mfi.version_type.ToLower(),
                            ReleaseDate = DateTime.Parse(mfi.date_published).ToUniversalTime().ToString(CultureInfo.CurrentCulture),
                            Timestamp = requestTime
                        };
                        writeApi.WriteMeasurement(downloadCount, WritePrecision.S, bucket.name,
                            _influxDbService.InfluxConfig.Org);
                    }
                });
            });
        }
    }
}