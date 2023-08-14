using System.Collections.Generic;
using de.z0rdak.moddev.util.Models.Platforms.Curseforge;
using de.z0rdak.moddev.util.Models.Platforms.Modrinth;

namespace de.z0rdak.moddev.util.Models.Platforms;

public class Platforms
{
    public List<string> UploadTargets { get; set; }
    public ModrinthInfo Modrinth { get; set; }
    public CurseforgeInfo Curseforge { get; set; }
}