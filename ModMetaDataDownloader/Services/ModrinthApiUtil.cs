using ModMetaDataDownloader.Model;
using RestSharp;

namespace ModMetaDataDownloader.Services;

public class ModrinthApiUtil
{
    private readonly RestClient _restClient;
    private readonly ILogger<ModrinthApiUtil> _logger;
    private readonly ModrinthConfig _modrinthConfig;

    public ModrinthApiUtil(IConfiguration configuration, ILogger<ModrinthApiUtil> logger)
    {
        
        _logger = logger;
        _modrinthConfig = configuration.GetSection("ModrinthApi").Get<ModrinthConfig>();
        _restClient = new RestClient(_modrinthConfig.Host);
    }


    public async Task<List<ModFileInfo>> GetModFileInfo(ModrinthModInfo mod)
    {
        var requestTime = DateTimeOffset.UtcNow.ToUniversalTime();
        var requestTimeStr = requestTime.ToString("yyyy'-'MM'-'dd'_'HH'-'mm'-'ss'-'fff'Z'");
        
        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation("[{Time}] Getting metadata from modrinth...", requestTimeStr);
        var request = new RestRequest($"/project/{mod.Id}/version");
        var response = await _restClient.GetAsync<List<ModFileInfo>>(request);
        if (response == null)
        {
            _logger.LogWarning("Error getting metadata at:  {time}", DateTimeOffset.UtcNow.ToUniversalTime()
                .ToString("yyyy'-'MM'-'dd'_'HH'-'mm'-'ss'-'fff'Z'"));
            return new List<ModFileInfo>();
        }

        return response;
    }
    
}