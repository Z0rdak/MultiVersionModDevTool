using System.Text.Json.Serialization;

namespace de.z0rdak.moddev.util;

public class UploadResponse
{
    [JsonPropertyName("id")] public int Id { get; set; }
}