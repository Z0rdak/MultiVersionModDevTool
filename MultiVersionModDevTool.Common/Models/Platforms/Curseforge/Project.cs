using System.Text.Json.Serialization;

namespace de.z0rdak.moddev.util.Models.Platforms.Curseforge;

public class Project
{
    public Project()
    {
    }

    public Project(string slug, string type = "requiredDependency")
    {
        Slug = slug;
        Type = type;
    }

    [JsonPropertyName("slug")] public string Slug { get; set; }

    [JsonPropertyName("type")] public string Type { get; set; }
}