using System.Globalization;
using HtmlAgilityPack;
using PuppeteerSharp;

namespace ModMetaDataDownloader.Services;

public class ModFileInfoScrapper
{
    private readonly ILogger<ModFileInfoScrapper> _logger;

    public ModFileInfoScrapper(IConfiguration configuration, ILogger<ModFileInfoScrapper> logger)
    {
        _logger = logger;
        ScrapConfig = configuration.GetSection("Scrapper").Get<ScrapConfig>();
    }

    public ScrapConfig ScrapConfig { get; }

    public async Task<Dictionary<string, string>> GetSiteContent()
    {
        var modSiteContent = new Dictionary<string, string>();
        foreach (var (_, mod) in ScrapConfig.Mods)
        {
            var requestTime = DateTimeOffset.UtcNow.ToUniversalTime();
            var url =
                $"{ScrapConfig.Host}/{mod.Name}/files/all?page=1&pageSize={mod.PageSize}";
            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation("[{time}] Getting mod download stats for '{modName}' (with Id={modId}) from curse ({url}) ...", requestTime.ToString(), mod.Name, mod.ModId, url);
            var modFileListContent = await ScrapSiteContent(url);
            modSiteContent.Add(mod.Name, modFileListContent);
        }

        return modSiteContent;
    }

    public async Task<string> GetSiteContent(string modname)
    {
        if (ScrapConfig.Mods.TryGetValue(modname, out var modInfo))
        {
            var url = $"{ScrapConfig.Host}/{modInfo.Name}/files/all?page=1&pageSize={modInfo.PageSize}";
            return await ScrapSiteContent(url);
        }

        throw new ArgumentOutOfRangeException(nameof(modname));
    }

    public async Task<List<FileElement>> ParseHtml(string html)
    {
        // Load the HTML content into an HtmlDocument
        var htmlDocument = new HtmlDocument();
        htmlDocument.LoadHtml(html);

        // Select the div with the class "files-table"
        var filesTableDiv = htmlDocument.DocumentNode.SelectSingleNode("//div[contains(@class, 'files-table')]");

        // Select all anchor elements with the class "file-row" within the filesTableDiv
        var fileRowAnchors = filesTableDiv.SelectNodes(".//a[contains(@class, 'file-row')]");

        var fileElements = new List<FileElement>();

        foreach (var anchorNode in fileRowAnchors)
        {
            // Extract the href attribute to get the fileId
            var href = anchorNode.GetAttributeValue("href", "");
            // Parse the URL to extract the fileId
            var fileId = href.Split('/').Last();

            // Select all top-level div elements within the anchor element
            var topLevelDivs = anchorNode.SelectNodes("./div");

            // Check if topLevelDivs are found
            if (topLevelDivs != null && topLevelDivs.Any())
            {
                var releaseTypeDiv = topLevelDivs[0];

                var releaseTypeE = releaseTypeDiv
                    .SelectSingleNode("./div")
                    .SelectSingleNode("./div");
                var releaseType = releaseTypeE.InnerText;

                var nameDiv = topLevelDivs[1];
                var name = nameDiv.SelectSingleNode("./span").InnerText;

                var releaseDateDiv = topLevelDivs[2];
                var dateString = releaseDateDiv.SelectSingleNode("./span").SelectSingleNode("./span").InnerText;
                var date = ParseDate(dateString);

                var sizeDiv = topLevelDivs[3];
                var size = sizeDiv.SelectSingleNode("./span").InnerText;

                var versions = new List<string>();
                try
                {
                    var gameVersionDiv = topLevelDivs[4];
                    var gameVersionListDiv = gameVersionDiv
                        .SelectSingleNode("./div")
                        .SelectSingleNode("./div");
                    var versionUist = gameVersionListDiv.SelectSingleNode("./ul");
                    var versionLi = versionUist.SelectNodes("./li");
                    versions = versionLi.Select(li => li.InnerText).ToList();
                }
                catch (Exception e)
                {
                    versions.Add("n/a");
                    _logger.LogWarning("Error getting version list for '{fileId}': {e} It may be empty", fileId, e.Message);
                }


                var modLoaders = new List<string>();
                try
                {
                    var modLoaderDiv = topLevelDivs[5];
                    var modloaderListDiv = modLoaderDiv
                        .SelectSingleNode("./div")
                        .SelectSingleNode("./div");
                    var unsortedList = modloaderListDiv.SelectSingleNode("./ul");
                    var listItems = unsortedList.SelectNodes("./li");
                    modLoaders = listItems.Select(li => li.InnerText).ToList();
                }
                catch (Exception e)
                {
                    modLoaders.Add("n/a");
                    _logger.LogWarning("Error getting modloader list for '{fileId}': {e} It may be empty", fileId, e.Message);
                }


                var downloadDiv = topLevelDivs[6];
                var downloadCountDiv = downloadDiv
                    .SelectSingleNode("./div")
                    .SelectSingleNode("./div");
                var downloadCount = int.Parse(downloadCountDiv.InnerText.Replace(".", ""));

                var fileElement = new FileElement
                {
                    FileId = fileId,
                    Name = name,
                    Size = size,
                    ReleaseType = releaseType,
                    ReleaseDate = date,
                    Versions = versions,
                    ModLoader = modLoaders,
                    DownloadCount = downloadCount
                };
                fileElements.Add(fileElement);
            }
            else
            {
                // No top-level div elements found within the anchor element
                Console.WriteLine("No top-level div elements found within the anchor element.");
            }
        }

        return await Task.FromResult(fileElements);
    }

    private async Task<string> ScrapSiteContent(string url)
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


    private DateTime ParseDate(string dateString)
    {
        try
        {
            var parsedDate = DateTime.Parse(dateString, // "MMM dd, yyyy",
                CultureInfo.CurrentCulture);
            return parsedDate;
        }
        catch (Exception e)
        {
            _logger.LogWarning("{e}", e.Message);
            return DateTime.MinValue;
        }
    }
}