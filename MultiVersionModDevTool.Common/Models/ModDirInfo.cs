using System.Collections.Generic;
using System.IO;

namespace de.z0rdak.moddev.util;

public class ModDirInfo
{
    public ModDirInfo(List<string> tokens, DirectoryInfo dir)
    {
        Name = tokens[0].ToLower();
        ModLoader = tokens[1].ToLower();
        Version = tokens[2].ToLower();
        DirInfo = dir;
    }

    public string Name { get; set; }
    public string ModLoader { get; set; }
    public string Version { get; set; }
    public DirectoryInfo DirInfo { get; set; }
}