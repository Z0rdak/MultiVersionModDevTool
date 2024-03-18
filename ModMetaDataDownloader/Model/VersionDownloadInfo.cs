namespace ModMetaDataDownloader;

public class VersionDownloadInfo
{
    public VersionDownloadInfo()
    {
    }

    public VersionDownloadInfo(File file)
    {
        id = file.id;
        display = file.display;
        type = file.type;
        version = file.version;
        versions = file.versions;
        downloads = file.downloads;
        uploaded_at = file.uploaded_at;
    }

    public int id { get; set; }
    public string display { get; set; }
    public string type { get; set; }
    public string version { get; set; }
    public string[] versions { get; set; }
    public int downloads { get; set; }
    public string uploaded_at { get; set; }
}