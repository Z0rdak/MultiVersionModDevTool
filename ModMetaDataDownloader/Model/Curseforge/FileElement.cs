﻿namespace ModMetaDataDownloader;

public class FileElement
{
    public string FileId { get; set; }
    public string ReleaseType { get; set; }
    public string Name { get; set; }
    public string Size { get; set; }
    public DateTime ReleaseDate { get; set; }
    public List<string> Versions { get; set; }
    public List<string> ModLoader { get; set; }
    public int DownloadCount { get; set; }
}