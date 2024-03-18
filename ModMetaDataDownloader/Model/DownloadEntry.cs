namespace ModMetaDataDownloader;

public class DownloadEntry
{
    public DownloadEntry(int downloads, string queryDate)
    {
        this.downloads = downloads;
        query_date = queryDate;
    }

    public DownloadEntry()
    {
    }

    public int downloads { get; set; }
    public string query_date { get; set; }
}