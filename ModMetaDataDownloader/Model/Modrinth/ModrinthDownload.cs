using InfluxDB.Client.Core;

namespace ModMetaDataDownloader.Model;

[Measurement("modrinth-downloads")]
public class ModrinthDownload
{
    [Column("loader", IsTag = true)] public string ModLoader { get; set; }
    [Column("loaders", IsTag = true)] public string ModLoaders { get; set; }
    [Column("mc-version", IsTag = true)] public string MinecraftVersion { get; set; }
    [Column("mc-versions", IsTag = true)] public string MinecraftVersions { get; set; }
    [Column("mod-version", IsTag = true)] public string ModVersion { get; set; }
    [Column("file-name", IsTag = true)] public string FileName { get; set; }
    [Column("file-id", IsTag = true)] public string FileId { get; set; }
    [Column("release-type", IsTag = true)] public string ReleaseType { get; set; }
    [Column("release-date", IsTag = true)] public string ReleaseDate { get; set; }
    [Column("count")] public int Count { get; set; }
    [Column(IsTimestamp = true)] public DateTimeOffset Timestamp { get; set; }
}