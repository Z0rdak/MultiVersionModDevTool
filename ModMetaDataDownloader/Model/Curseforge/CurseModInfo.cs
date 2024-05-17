namespace ModMetaDataDownloader;

/**
 * Type exposed by the curseforge mod API
 * But since the curseforge mod api is still broken AF, this is not used
 * E.g. the download count for the mods is just wrong
 */
public class CurseModInfo
{
    public int id { get; set; }
    public string title { get; set; }
    public string summary { get; set; }
    public string description { get; set; }
    public string game { get; set; }
    public string type { get; set; }
    public Urls urls { get; set; }
    public string thumbnail { get; set; }
    public string created_at { get; set; }
    public Downloads downloads { get; set; }
    public string license { get; set; }
    public string donate { get; set; }
    public string[] categories { get; set; }
    public Members[] members { get; set; }
    public object[] links { get; set; }
    public File[] files { get; set; }
    public Download download { get; set; }
}

public class Urls
{
    public string curseforge { get; set; }
    public string project { get; set; }
}

public class Downloads
{
    public int monthly { get; set; }
    public int total { get; set; }
}

public class Members
{
    public string title { get; set; }
    public string username { get; set; }
    public int id { get; set; }
}

public class File
{
    public int id { get; set; }
    public string url { get; set; }
    public string display { get; set; }
    public string name { get; set; }
    public string type { get; set; }
    public string version { get; set; }
    public int filesize { get; set; }
    public string[] versions { get; set; }
    public int downloads { get; set; }
    public string uploaded_at { get; set; }
}

public class Download
{
    public int id { get; set; }
    public string url { get; set; }
    public string display { get; set; }
    public string name { get; set; }
    public string type { get; set; }
    public string version { get; set; }
    public int filesize { get; set; }
    public string[] versions { get; set; }
    public int downloads { get; set; }
    public string uploaded_at { get; set; }
}