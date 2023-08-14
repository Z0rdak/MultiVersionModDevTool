namespace de.z0rdak.moddev.util.Models.Environment;

public class ResourcePack
{
    public string Dir { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public bool CreateAfterFileSync { get; set; }
    public bool AutoZip { get; set; }
    public Dictionary<string, int> PackVersion { get; set; }
}