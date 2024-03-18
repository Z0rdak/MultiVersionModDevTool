using Coravel.Invocable;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace ModMetaDataDownloader.Services;

public class WriteModFileInfoInvocable : IInvocable
{
    private readonly InfluxDbService _influxDbService;
    private readonly ILogger<WriteModFileInfoInvocable> _logger;
    private readonly ModFileInfoScrapper _modFileInfoScrapper;

    public WriteModFileInfoInvocable(InfluxDbService influxDbService, ModFileInfoScrapper modFileInfoScrapper,
        ILogger<WriteModFileInfoInvocable> logger)
    {
        _influxDbService = influxDbService;
        _modFileInfoScrapper = modFileInfoScrapper;
        _logger = logger;
    }

    public async Task Invoke()
    {
        var requestTime = DateTimeOffset.UtcNow.ToUniversalTime();
        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation("[{time}] Getting data from curse...", requestTime.ToString());

        var modFileElements = new Dictionary<string, List<FileElement>>();

        var modFileTables = await _modFileInfoScrapper.GetSiteContent();
        foreach (var (modName, fileTableHtml) in modFileTables)
        {
            var fileElements = await _modFileInfoScrapper.ParseHtml(fileTableHtml);
            modFileElements.Add(modName, fileElements);
        }

        // write into mod specific buckets
        foreach (var (modName, fileElements) in modFileElements)
        {
            await Task.Run(() =>
            {
                var bucket = _influxDbService.InfluxConfig.Buckets[modName];
                _logger.LogInformation("Writing data for '{modName}' to bucket '{bucket}'...", modName, bucket);
                fileElements.ForEach(async e =>
                {
                    _influxDbService.Write(writeApi =>
                    {
                        var downloadCount = new DownloadCount
                            { FileId = e.FileId, Count = e.DownloadCount, Timestamp = requestTime };
                        writeApi.WriteMeasurement(downloadCount, WritePrecision.S, bucket,
                            _influxDbService.InfluxConfig.Org);
                    });
                });

                fileElements
                    .Where(f => f.ModLoader.Contains("Fabric"))
                    .ToList()
                    .ForEach(f =>
                    {
                        _influxDbService.Write(writeApi =>
                        {
                            var downloadCount = new FabricDownloadCount
                                { FileId = f.FileId, Count = f.DownloadCount, Timestamp = requestTime };
                            writeApi.WriteMeasurement(downloadCount, WritePrecision.S, bucket,
                                _influxDbService.InfluxConfig.Org);
                        });
                    });

                fileElements
                    .Where(f => f.ModLoader.Contains("Forge"))
                    .ToList()
                    .ForEach(f =>
                    {
                        _influxDbService.Write(writeApi =>
                        {
                            var downloadCount = new ForgeDownloadCount
                                { FileId = f.FileId, Count = f.DownloadCount, Timestamp = requestTime };
                            writeApi.WriteMeasurement(downloadCount, WritePrecision.S, bucket,
                                _influxDbService.InfluxConfig.Org);
                        });
                    });

                // write measurement for version-modloader
                fileElements.ForEach(f =>
                {
                    _influxDbService.Write(writeApi =>
                    {
                        var versionStr = f.Versions[0].Replace(".", "-");
                        var point = PointData.Measurement($"{f.ModLoader[0]}_{versionStr}")
                            .Tag("fileId", f.FileId)
                            .Field("count", f.DownloadCount)
                            .Timestamp(requestTime, WritePrecision.S);
                        try
                        {
                            writeApi.WriteMeasurement(point, WritePrecision.S, bucket,
                                _influxDbService.InfluxConfig.Org);
                        } catch (Exception e)
                        {
                            _logger.LogError("Error writing measurement for {fileId}: {e}", f.FileId, e.Message);
                        }
                    });
                });
            });
        }
    }
}