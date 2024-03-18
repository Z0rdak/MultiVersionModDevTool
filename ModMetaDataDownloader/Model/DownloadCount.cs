using InfluxDB.Client.Core;

namespace ModMetaDataDownloader.Services;

[Measurement("downloadCount")]
public class DownloadCount
{
    [Column("fileId", IsTag = true)] public string FileId { get; set; }
    [Column("count")] public int Count { get; set; }
    [Column(IsTimestamp = true)] public DateTimeOffset Timestamp { get; set; }
}

[Measurement("downloadCount-fabric")]
public class FabricDownloadCount
{
    [Column("fileId", IsTag = true)] public string FileId { get; set; }
    [Column("count")] public int Count { get; set; }
    [Column(IsTimestamp = true)] public DateTimeOffset Timestamp { get; set; }
}

[Measurement("downloadCount-forge")]
public class ForgeDownloadCount
{
    [Column("fileId", IsTag = true)] public string FileId { get; set; }
    [Column("count")] public int Count { get; set; }
    [Column(IsTimestamp = true)] public DateTimeOffset Timestamp { get; set; }
}