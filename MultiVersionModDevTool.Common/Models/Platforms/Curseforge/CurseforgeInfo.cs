using System.Collections.Generic;

namespace de.z0rdak.moddev.util.Models.Platforms.Curseforge;

public class CurseforgeInfo : Platform
{
    public Dictionary<string, Project> Projects { get; set; }

    public string Token { get; set; }
    public int ModId { get; set; }

    public Dictionary<string, int> VersionIds { get; set; }

    public List<string> CommonIds { get; set; }

    public Dictionary<string, List<string>> IdsForVersion { get; set; }
}