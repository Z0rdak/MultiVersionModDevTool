using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace de.z0rdak.moddev.util.Models.Platforms.Modrinth;

public class ModrinthMetaData : ModMetaData
{
    public ModrinthMetaData(string versionName, string projectId, string versionNumber, string[] gameVersions,
        string versionType, string[] loaders, string changelog, string fileToUpload,
        List<Dependency>? deps) : base(changelog)
    {
        ProjectId = projectId;
        Name = versionName;
        VersionNumber = versionNumber;
        GameVersions = gameVersions;
        VersionType = versionType;
        Loaders = loaders;
        Featured = true;
        Status = "listed";
        RequestedStatus = "listed";
        FileParts = new[] { fileToUpload };
        PrimaryFile = fileToUpload;
        var depList = deps ?? new List<Dependency>();
        Dependencies = depList.ToArray();
    }

    public ModrinthMetaData()
    {
    }

    [JsonPropertyName("name")] public string Name { get; set; }

    [JsonPropertyName("version_number")] public string VersionNumber { get; set; }

    [JsonPropertyName("dependencies")] public Dependency[] Dependencies { get; set; }

    [JsonPropertyName("game_versions")] public string[] GameVersions { get; set; }

    [JsonPropertyName("version_type")] public string VersionType { get; set; }

    [JsonPropertyName("loaders")] public string[] Loaders { get; set; }

    [JsonPropertyName("featured")] public bool Featured { get; set; }

    [JsonPropertyName("status")] public string Status { get; set; }

    [JsonPropertyName("requested_status")] public string RequestedStatus { get; set; }

    [JsonPropertyName("project_id")] public string ProjectId { get; set; }

    [JsonPropertyName("file_parts")] public string[] FileParts { get; set; }

    [JsonPropertyName("primary_file")] public string PrimaryFile { get; set; }
}