using System.Collections.Generic;

namespace de.z0rdak.moddev.util.Models.Environment;

public class Source
{
    public string SrcDir { get; set; }
    public string SrcDirFilterRegex { get; set; }
    public List<string> Exclude { get; set; }
}