namespace de.z0rdak.moddev.util.Models.Modloaders;

public interface Modloader
{
}

public class ModLoaders : Modloader
{
    public Fabric.Fabric Fabric { get; set; }
    public Forge.Forge Forge { get; set; }
}