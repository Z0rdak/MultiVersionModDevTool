namespace ModMetaDataDownloader;

public class InfluxConfig
{
    public string Token { get; set; }
    public string Org { get; set; }
    public string Host { get; set; }
    public Dictionary<string, BucketInfo> Buckets { get; set; }
    
}

public class BucketInfo
{
    public string name { get; set; }
    public string desc { get; set; }
}