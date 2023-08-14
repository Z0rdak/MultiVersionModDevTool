using System.Collections.Generic;
using System.IO;

namespace de.z0rdak.moddev.util;

public class ModFileInfo
{
    public ModFileInfo(List<string> tokens, FileInfo jarFileInfo)
    {
        Name = tokens[0];
        MinecraftVersion = tokens[1];
        ModVersion = $"{tokens[2]}-{tokens[3]}";
        ModLoader = tokens[4].Split('.')[0];
        FileInfo = jarFileInfo;
    }

    public string Name { get; set; }
    public string ModLoader { get; set; }
    public string MinecraftVersion { get; set; }
    public string ModVersion { get; set; }
    public FileInfo FileInfo { get; set; }
}