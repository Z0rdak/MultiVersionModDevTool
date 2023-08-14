using System.Text.Json.Serialization;

namespace de.z0rdak.moddev.util.Models.Platforms;

public abstract class ModMetaData
{
    public ModMetaData(string changelog)
    {
        Changelog = changelog;
    }

    public ModMetaData()
    {
    }

    [JsonPropertyName("changelog")] public string Changelog { get; set; }
}