using System.Collections.Generic;

namespace de.z0rdak.moddev.util.Models.Platforms.Modrinth;

public class ModrinthInfo : Platform
{
    public string Token { get; set; }

    public string ModId { get; set; }
    
    public string UserAgent { get; set; }

    public Dictionary<string, Dependency> Dependencies { get; set; }
}