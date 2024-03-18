namespace ModMetaDataDownloader;

public class InfluxConfig
{
    public string Token { get; set; }
    public string Org { get; set; }
    public string Host { get; set; }
    public Dictionary<string, string> Buckets { get; set; }
}