using System.Collections.Generic;

namespace de.z0rdak.moddev.util.Models.Environment;

public class DefaultFileSync
{
    public string SyncSrcDir { get; set; }
    public bool SyncResourcePack { get; set; }
    public List<string> FilesToSync { get; set; }
}