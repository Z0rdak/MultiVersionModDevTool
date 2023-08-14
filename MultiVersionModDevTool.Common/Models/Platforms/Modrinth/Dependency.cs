using System.Text.Json.Serialization;

namespace de.z0rdak.moddev.util.Models.Platforms.Modrinth;

public class Dependency
{
    public Dependency()
    {
    }

    public Dependency(string projectId, string fileName, string dependencyType, string versionId = "")
    {
        VersionId = versionId;
        ProjectId = projectId;
        FileName = fileName;
        DependencyType = dependencyType;
    }

    [JsonPropertyName("version_id")] public string VersionId { get; set; }

    [JsonPropertyName("project_id")] public string ProjectId { get; set; }

    [JsonPropertyName("file_name")] public string FileName { get; set; }

    [JsonPropertyName("dependency_type")] public string DependencyType { get; set; }
}