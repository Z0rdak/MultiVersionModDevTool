using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace de.z0rdak.moddev.util.Models.Platforms.Curseforge;

public class CurseforgeMetaData : ModMetaData
{
    public CurseforgeMetaData()
    {
    }

    public CurseforgeMetaData(string versionName, int[] gameVersions, string changelog, string releaseType,
        List<Project>? projects,
        string changelogType = "markdown") : base(changelog)
    {
        DisplayName = versionName;
        ChangelogType = changelogType;
        GameVersions = gameVersions;
        ReleaseType = releaseType;
        Relations = new Relations
        {
            Projects = projects ?? new List<Project>()
        };
    }

    [JsonPropertyName("changelogType")] public string ChangelogType { get; set; }

    [JsonPropertyName("displayName")] public string DisplayName { get; set; }

    [JsonPropertyName("parentFileID")] public int ParentFileID { get; set; }

    [JsonPropertyName("gameVersions")] public int[] GameVersions { get; set; }

    [JsonPropertyName("releaseType")] public string ReleaseType { get; set; }

    [JsonPropertyName("relations")] public Relations Relations { get; set; }
}

public class Relations
{
    [JsonPropertyName("projects")] public List<Project> Projects { get; set; }
}