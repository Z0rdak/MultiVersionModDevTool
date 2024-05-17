namespace ModMetaDataDownloader;

public class ModrinthConfig
{
    public Dictionary<string, ModrinthModInfo> Mods { get; set; }
    public string Host { get; set; }
}

public class ModrinthModInfo
{
    public string ApiKey { get; set; }
    public string Slug { get; set; }
    public string Id { get; set; }
}