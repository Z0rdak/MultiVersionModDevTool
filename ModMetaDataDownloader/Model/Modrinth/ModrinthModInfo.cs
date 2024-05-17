namespace ModMetaDataDownloader.Model;

public class ModFileInfo
{
    public string[] game_versions { get; set; }
    public string[] loaders { get; set; }
    public string id { get; set; }
    public string project_id { get; set; }
    public string name { get; set; }
    public string version_number { get; set; }
    public string date_published { get; set; }
    public DateTimeOffset release_date { get; set; }
    public int downloads { get; set; }
    public string version_type { get; set; }
}
