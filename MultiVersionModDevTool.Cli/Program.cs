using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using de.z0rdak.moddev.util.Models;
using de.z0rdak.moddev.util.Models.Environment;
using de.z0rdak.moddev.util.Models.Platforms;
using de.z0rdak.moddev.util.Models.Platforms.Curseforge;
using de.z0rdak.moddev.util.Models.Platforms.Modrinth;

namespace de.z0rdak.moddev.util;

internal class Program
{
    private const string Forge = "forge";
    private const string Fabric = "fabric";

    // TODO: Create Resource Packs and Upload them somewhere
    // TODO: Generalize file sync (sync dirs, config for syncing)
    // TODO: bump version
    // TODO: commit and push new version
    private static readonly Dictionary<int, string> CliMenuOptions = new()
    {
        [0] = "Sync files",
        [1] = "Build jars",
        [2] = "Copy jars",
        [3] = "Upload jars",
        [-1] = "Exit"
    };

    private static readonly Dictionary<int, Action> CliMenuActions = new()
    {
        [0] = SyncFiles,
        [1] = BuildJars,
        [2] = CopyJars,
        [3] = UploadJars,
        [-1] = () => System.Environment.Exit(0)
    };

    private static List<ModDirInfo> ModDirInfos = new();

    private static ModEnvironment Environment { get; set; }
    private static ModInfo ModInfo { get; set; }
    private static Platforms Platforms { get; set; }
    private static ModrinthInfo ModrinthInfo { get; set; }
    private static CurseforgeInfo CurseforgeInfo { get; set; }

    public static void Main(string[] args)
    {
        Environment = SettingsUtil.LoadEnvironmentSettings(AppDomain.CurrentDomain.BaseDirectory);
        ModInfo = SettingsUtil.LoadModInfoSettings(AppDomain.CurrentDomain.BaseDirectory);
        Platforms = SettingsUtil.LoadPlatformSettings(AppDomain.CurrentDomain.BaseDirectory);
        ModrinthInfo = Platforms.Modrinth;
        CurseforgeInfo = Platforms.Curseforge;

        var directoryInfo = new DirectoryInfo(Environment.Source.SrcDir);

        if (!directoryInfo.Exists)
        {
            Console.WriteLine($"Directory '{directoryInfo.FullName}' does not exist.");
            return;
        }

        ModDirInfos = directoryInfo.GetDirectories()
            .Where(d => Regex.IsMatch(d.Name, Environment.Source.SrcDirFilterRegex))
            .Where(d => !Environment.Source.Exclude.Contains(d.Name))
            .Select(dir => new ModDirInfo(dir.Name.Split('-').ToList(), dir))
            .Where(dir => dir.Name == ModInfo.ModId)
            .ToList();

        if (ModDirInfos.Count == 0)
        {
            Console.WriteLine($"No matching directories for mod with id '{ModInfo.ModId}' found.");
            return;
        }

        while (true)
        {
            Console.WriteLine("\n== Multi mod version development tool ==");
            Console.WriteLine("Select action: ");
            foreach (var (idx, action) in CliMenuOptions) Console.WriteLine($"({idx}): {action}");

            var selectedAction = Console.ReadLine();
            if (int.TryParse(selectedAction, out var actionIdx) && CliMenuOptions.ContainsKey(actionIdx))
                CliMenuActions[actionIdx].Invoke();
        }
    }

    private static List<ModDirInfo> SearchModDirs(List<DirectoryInfo> directories, string modId)
    {
        ModDirInfos = directories
            .Select(dir => new ModDirInfo(dir.Name.Split('-').ToList(), dir))
            .Where(dir => dir.Name == modId)
            .OrderBy(d => d.ModLoader)
            .ThenBy(d => d.Version)
            .ToList();

        if (ModDirInfos.Count == 0)
        {
            Console.WriteLine($"No directories matching the modId '{modId}' were found.");
            return new List<ModDirInfo>();
        }

        Console.WriteLine($"Found {ModDirInfos.Count} directories matching the modId '{modId}':");
        return ModDirInfos;
    }

    private static void CopyFilesRecursively(string sourcePath, string targetPath)
    {
        //Now Create all of the directories
        foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
        {
            Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));
        }

        //Copy all the files & Replaces any files with the same name
        foreach (string source in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
        {
            var destPath = source.Replace(sourcePath, targetPath);
            Console.WriteLine($"Copying file {source} to {destPath}...");
            File.Copy(source, destPath, true);
        }
    }

    private static void SyncFiles()
    {
        ModDirInfo? modDirInfo = ModDirInfos.Find(
            e => $"{e.Name}-{e.ModLoader}-{e.Version}".Equals(Environment.DefaultFileSync.SyncSrcDir,
                StringComparison.OrdinalIgnoreCase));
        if (modDirInfo == null)
        {
            Console.WriteLine($"Unable to find dir '{Environment.DefaultFileSync.SyncSrcDir}'");
            return;
        }
        Console.WriteLine("Syncing following files/directories: ");
        Environment.DefaultFileSync.FilesToSync.ForEach(f => Console.WriteLine($"\t-{f}"));
        Console.WriteLine("Continue? yes(Enter) - no(Esc)");
        var input = Console.ReadKey();
        switch (input.Key)
        {
            case ConsoleKey.Enter:
            {
                Environment.DefaultFileSync.FilesToSync.ForEach(path =>
                {
                    var srcPath = $"{modDirInfo.DirInfo.FullName}/{path}";
                    if (File.Exists(srcPath) || Directory.Exists(srcPath))
                    {
                        FileAttributes attr = File.GetAttributes(srcPath);
                        if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                        {
                            // copy directory to other mod environments
                            ModDirInfos.ForEach(mdi =>
                            {
                                var dirTargetPath = $"{mdi.DirInfo.FullName}/{path}";
                                if (srcPath != dirTargetPath)
                                {
                                    CopyFilesRecursively(srcPath, dirTargetPath);
                                }
                            });
                        }
                        if ((attr & FileAttributes.Normal) == FileAttributes.Normal)
                        {
                            // copy file to other mod environments
                            var srcfileInfo = new FileInfo(srcPath);
                            ModDirInfos.ForEach(mdi =>
                            {
                                var fileTargetPath = $"{mdi.DirInfo.FullName}/{path}";
                                if (srcPath != fileTargetPath)
                                {
                                    var targetfileInfo = new FileInfo(fileTargetPath);
                                    Console.WriteLine(
                                        $"Copying file {srcfileInfo.Name} to {targetfileInfo.FullName}...");
                                    File.Copy(srcPath, fileTargetPath, true);
                                }
                            });
                        }
                    }
                    else
                    {
                        Console.WriteLine($"File or directory {srcPath} does not exist.");
                    }
                });
            };
                break;
            case ConsoleKey.Escape:
            default:
                Console.WriteLine($"File sync aborted!");
                break;
        }
    }

    // TODO: Return error info if any
    public static void BuildJars()
    {
        var buildProcesses = new List<Process>();
        ModDirInfos.ForEach(mdi =>
        {
            if (Environment.Build.ClearExisting)
                DeleteDirectoryContent($"{mdi.DirInfo.FullName}/{Environment.Build.Dir}");

            var includeGradleSettings = File.Exists($"{mdi.DirInfo.FullName}/settings.gradle");
            var gradleSettings = $"{(includeGradleSettings ? "-c settings.gradle" : "")}";
            Console.WriteLine($"Building jar for {mdi.ModLoader} - {mdi.Version}");
            var startInfo = new ProcessStartInfo
            {
                WorkingDirectory = mdi.DirInfo.FullName,
                WindowStyle = ProcessWindowStyle.Normal,
                FileName = $"{mdi.DirInfo.FullName}/gradlew",
                UseShellExecute = false,
                Arguments = $"build -b {mdi.DirInfo.FullName}/build.gradle {gradleSettings}"
            };

            var buildProcess = Process.Start(startInfo);
            if (buildProcess != null) buildProcesses.Add(buildProcess);
        });
        buildProcesses.ForEach(p => p.WaitForExit());

        if (Environment.Build.VerifyBuild)
            // TODO: Verify existing file in build dir 
            // TODO: Verify jar meta data
            buildProcesses.ForEach(p =>
            {
                if (p.ExitCode != 0) Console.Write("Error while building");
            });

        // TODO: Check build results / process exit codes
        Console.Write("Finished building");
    }

    /// <summary>
    ///     Delete existing files and directories within output dir
    /// </summary>
    /// <param name="directory"></param>
    private static void DeleteDirectoryContent(string directory)
    {
        var di = new DirectoryInfo(directory);
        if (!di.Exists) return;
        di.GetFiles().ToList().ForEach(f => f.Delete());
        di.GetDirectories().ToList().ForEach(d => d.Delete(true));
    }

    private static void CopyJars()
    {
        Console.WriteLine($"Copying jars to '{Environment.Output.Dir}'...");

        if (!string.IsNullOrEmpty(Environment.Output.Dir) && !Directory.Exists(Environment.Output.Dir))
            Directory.CreateDirectory(Environment.Output.Dir);
        else
            DeleteDirectoryContent(Environment.Output.Dir);

        ModDirInfos.ForEach(mdi =>
        {
            var jarBuildDir = new DirectoryInfo($"{mdi.DirInfo.FullName}/{Environment.Build.Dir}");

            var fileInfos = jarBuildDir.GetFiles();
            if (fileInfos.Length > 0)
            {
                var newJar = fileInfos
                    .Where(f => !f.Name.Contains("sources"))
                    .OrderByDescending(f => f.LastWriteTime)
                    .First();
                if (newJar.Exists)
                {
                    Console.WriteLine($"Copying jar {newJar.FullName} to {Environment.Output.Dir}..");
                    File.Copy(newJar.FullName, $"{Environment.Output.Dir}/{newJar.Name}", true);
                }
                else
                {
                    Console.WriteLine($"Unable to find jar {newJar.FullName}. Check the build output for details");
                }
            }
            else
            {
                Console.WriteLine($"No files found in {mdi.DirInfo.FullName}.");
            }
        });
    }

    public static void UploadJars()
    {
        Console.WriteLine($"Uploading jars from '{Environment.Output.Dir}'...");

        if (!string.IsNullOrEmpty(Environment.Output.Dir) && Directory.Exists(Environment.Output.Dir))
        {
            var uploadDirInfo = new DirectoryInfo(Environment.Output.Dir);
            var modFileInfos = uploadDirInfo.GetFiles()
                .Where(f => f.Extension.ToLower().Contains("jar"))
                .Select(jar => new ModFileInfo(jar.Name.Split('-').ToList(), jar))
                .OrderBy(mfi => mfi.ModLoader)
                .ThenBy(mfi => mfi.MinecraftVersion)
                .ToList();
            modFileInfos.ForEach(async modFile => await UploadFile(modFile));
        }
    }

    public static async Task UploadFile(ModFileInfo modFile)
    {
        var versionName = $"{modFile.MinecraftVersion}-{modFile.ModVersion}-{modFile.ModLoader}";
        var curseforgeMetaData = BuildCurseforgeMetaData(modFile, versionName);
        var modrinthMetaData = BuildModrinthMetaData(modFile, versionName);

        var uploadTasks = new List<Task<bool>>();
        if (Platforms.UploadTargets.Contains("Modrinth"))
            uploadTasks.Add(Uploader.UploadFileToModrinth(ModrinthInfo, modrinthMetaData, modFile.FileInfo));
        if (Platforms.UploadTargets.Contains("Curseforge"))
            uploadTasks.Add(Uploader.UploadFileToCurse(CurseforgeInfo, curseforgeMetaData, modFile.FileInfo));
        await Task.WhenAll(uploadTasks);
    }

    private static CurseforgeMetaData BuildCurseforgeMetaData(ModFileInfo modFile, string versionName)
    {
        var curseforgeVersionIds = GetCurseforgeVersionIds(modFile);
        var curseforgeDependencies = modFile.ModLoader == Fabric
            ? CurseforgeInfo.Projects.Select(dep => dep.Value).ToList()
            : new List<Project>();
        var curseforgeMetaData = new CurseforgeMetaData(
            versionName,
            curseforgeVersionIds.ToArray(),
            ModInfo.Changelog,
            ModInfo.ReleaseType,
            curseforgeDependencies);
        return curseforgeMetaData;
    }

    private static ModrinthMetaData BuildModrinthMetaData(ModFileInfo modFile, string versionName)
    {
        var gameVersions = new[] { modFile.MinecraftVersion };
        var modloaders = new[] { modFile.ModLoader };
        var modrinthDependencies = modFile.ModLoader == Fabric
            ? ModrinthInfo.Dependencies.Select(dep => dep.Value).ToList()
            : new List<Dependency>();

        var modrinthMetaData = new ModrinthMetaData(
            versionName, ModrinthInfo.ModId,
            modFile.ModVersion,
            gameVersions,
            ModInfo.ReleaseType,
            modloaders,
            ModInfo.Changelog,
            modFile.FileInfo.FullName,
            modrinthDependencies);
        return modrinthMetaData;
    }

    private static List<int> GetCurseforgeVersionIds(ModFileInfo modFile)
    {
        var versionIds = CurseforgeInfo.CommonIds
            .Select(id => CurseforgeInfo.VersionIds[id])
            .ToList();
        versionIds.Add(CurseforgeInfo.VersionIds[modFile.ModLoader]);
        var idsForVersion = CurseforgeInfo.IdsForVersion[modFile.MinecraftVersion]
            .Select(ids => CurseforgeInfo.VersionIds[ids]).ToList();
        versionIds.AddRange(idsForVersion);

        // FIXME: Should not be needed but in some cases both modloaders ids get added 
        if (modFile.ModLoader == Fabric && versionIds.Contains(CurseforgeInfo.VersionIds[Forge]))
            versionIds.Remove(CurseforgeInfo.VersionIds[Forge]);

        if (modFile.ModLoader == Forge && versionIds.Contains(CurseforgeInfo.VersionIds[Fabric]))
            versionIds.Remove(CurseforgeInfo.VersionIds[Fabric]);

        return versionIds;
    }

    private static void PrintModDirs(List<ModDirInfo> modDirs)
    {
        var idx = 0;
        foreach (var d in modDirs)
        {
            Console.WriteLine($"({idx}) {d.ModLoader} - {d.Version}");
            idx++;
        }
    }
}