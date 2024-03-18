using System.Text.Json;
using ModMetaDataDownloader.Services;
using PuppeteerSharp;
using RestSharp;

namespace ModMetaDataDownloader;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly RestClient _restClient;

    public Worker(ILogger<Worker> logger, InfluxDbService influxDbService)
    {
        var options = new RestClientOptions("https://api.cfwidget.com")
        {
            //Authenticator = new HttpBasicAuthenticator("username", "password")
        };
        _restClient = new RestClient(options);
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var delayInSec = 1000 * 60 * 60 * 24;
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(delayInSec, stoppingToken);
            var requestTime = DateTimeOffset.UtcNow.ToUniversalTime();
            var requestTimeStr = requestTime.ToString("yyyy'-'MM'-'dd'_'HH'-'mm'-'ss'-'fff'Z'");
            try
            {
                var request = new RestRequest("/minecraft/mc-mods/yawp");
                var response = await _restClient.GetAsync<CurseModInfo>(request, stoppingToken);
                if (_logger.IsEnabled(LogLevel.Information))
                    _logger.LogInformation("Getting metadata from curse at: {time}", requestTimeStr);
                if (response == null)
                {
                    _logger.LogWarning("Error getting metadata at:  {time}", DateTimeOffset.UtcNow.ToUniversalTime()
                        .ToString("yyyy'-'MM'-'dd'_'HH'-'mm'-'ss'-'fff'Z'"));
                    continue;
                }

                var versions = new Dictionary<int, VersionDownloadInfo>();
                response.files.ToList()
                    .ForEach(f => versions.Add(f.id, new VersionDownloadInfo(f)));
                _logger.LogInformation("Successfully got data at:  {time}", DateTimeOffset.UtcNow.ToUniversalTime()
                    .ToString("yyyy'-'MM'-'dd'_'HH'-'mm'-'ss'-'fff'Z'"));
                var path = string.Empty;
                if (OperatingSystem.IsWindows()) path = @"D:\YAWP\metadata";

                if (OperatingSystem.IsLinux()) path = @"/home/pi/yawp/metadata";

                var metadataFilePath = Path.Combine(path, "version-metadata.json");

                if (!System.IO.File.Exists(metadataFilePath))
                {
                    var streamWriter = System.IO.File.CreateText(metadataFilePath);
                    streamWriter.Write("{}");
                    streamWriter.Flush();
                    streamWriter.Close();
                    streamWriter.Dispose();
                }

                _logger.LogInformation("Reading metadata from:  {path}", metadataFilePath);
                var metadataJson = System.IO.File.ReadAllText(metadataFilePath);
                var versionMap = JsonSerializer.Deserialize<Dictionary<int, VersionDownloadInfo>>(metadataJson);
                if (versionMap == null)
                {
                    _logger.LogWarning("Error reading version metadata file:  {metadataFilePath}", metadataFilePath);
                    continue;
                }

                foreach (var versionInfo in versions)
                    if (!versionMap.ContainsKey(versionInfo.Key))
                        versionMap.Add(versionInfo.Key, versionInfo.Value);

                // save version-metadata.json back
                var versionJson =
                    JsonSerializer.Serialize(versionMap, new JsonSerializerOptions { WriteIndented = true });
                _logger.LogInformation("Saving metadata to:  {path}", metadataFilePath);
                System.IO.File.WriteAllText(metadataFilePath, versionJson);

                // Iterate through individual download files
                foreach (var (versionId, versionInfo) in versionMap)
                {
                    var downloadsFilePath = Path.Combine(path, $"{versionId}-downloads.json");


                    if (!System.IO.File.Exists(downloadsFilePath))
                    {
                        var streamWriter = System.IO.File.CreateText(downloadsFilePath);
                        streamWriter.Write("[]");
                        streamWriter.Flush();
                        streamWriter.Close();
                        streamWriter.Dispose();

                        var downloadEntry = new DownloadEntry(versionInfo.downloads, requestTimeStr);
                        var downloadEntries = new List<DownloadEntry> { downloadEntry };
                        AppendDownload(downloadEntries, downloadsFilePath, stoppingToken);
                    }
                    else
                    {
                        var versionDownloadJson = System.IO.File.ReadAllText(downloadsFilePath);
                        var downloadInfo = JsonSerializer.Deserialize<List<DownloadEntry>>(versionDownloadJson);
                        if (downloadInfo == null)
                        {
                            _logger.LogWarning("Error reading version download file: {downloadsFilePath}",
                                downloadsFilePath);
                            continue;
                        }

                        var downloadEntry = new DownloadEntry(versionInfo.downloads, requestTimeStr);
                        downloadInfo.Add(downloadEntry);
                        AppendDownload(downloadInfo, downloadsFilePath, stoppingToken);
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogWarning("Error getting metadata:  {e}", e.Message);
                continue;
            }

            var nextTime = requestTime.AddMilliseconds(delayInSec);
            _logger.LogInformation("Going to sleep. Next request at:  {}",
                nextTime.ToString("yyyy'-'MM'-'dd'_'HH'-'mm'-'ss'-'fff'Z'"));
            await Task.Delay(delayInSec, stoppingToken);
        }
    }


    private async Task<string> GetSiteContent(string url)
    {
        // Launch headless Chromium browser
        var launchOptions = new LaunchOptions
        {
            Headless = false
        };
        await new BrowserFetcher().DownloadAsync();
        await using (var browser = await Puppeteer.LaunchAsync(launchOptions))
        {
            // Create a new page
            await using (var page = await browser.NewPageAsync())
            {
                // Navigate to the desired webpage
                await page.GoToAsync(url, waitUntil: new[] { WaitUntilNavigation.Networkidle2 });

                await page.SetViewportAsync(new ViewPortOptions { Width = 1920, Height = 1080, IsMobile = false });

                // Wait for the table with class "files-table" to appear on the page
                var fileTableSelector = ".files-table";
                await page.WaitForSelectorAsync(fileTableSelector, new WaitForSelectorOptions { Timeout = 5000 });

                // Get the HTML content of the page
                return await page.GetContentAsync();
            }
        }
    }


    private void AppendDownload(List<DownloadEntry> downloadEntries, string targetPath,
        CancellationToken stoppingToken)
    {
        var downloadInfoJson =
            JsonSerializer.Serialize(downloadEntries, new JsonSerializerOptions { WriteIndented = true });
        _logger.LogInformation("Appending data to:  {path}", targetPath);
        System.IO.File.WriteAllText(targetPath, downloadInfoJson);
    }
}