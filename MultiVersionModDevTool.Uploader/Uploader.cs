using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using de.z0rdak.moddev.util;
using de.z0rdak.moddev.util.Models;
using de.z0rdak.moddev.util.Models.Modloaders.Fabric;
using de.z0rdak.moddev.util.Models.Modloaders.Forge;
using de.z0rdak.moddev.util.Models.Platforms.Curseforge;
using de.z0rdak.moddev.util.Models.Platforms.Modrinth;
using RestSharp;

namespace de.z0rdak.moddev.uploader;

public static class Uploader
{
    public const string Forge = "forge";
    public const string Fabric = "fabric";

    public static async Task<bool> UploadFileToModrinth(ModrinthInfo modrinthInfo, ModrinthMetaData metaData,
        FileInfo filePath)
    {
        try
        {
            var metajson = JsonSerializer.Serialize(metaData, new JsonSerializerOptions
            {
                WriteIndented = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
            });
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.modrinth.com/v2/version");
            request.Headers.Add("User-Agent", modrinthInfo.UserAgent);
            request.Headers.Add("Authorization", modrinthInfo.Token);
            var content = new MultipartFormDataContent();
            content.Add(new StringContent(metajson), "data");
            content.Add(new StreamContent(File.OpenRead(filePath.FullName)), "file", filePath.Name);
            request.Content = content;

            var response = await client.SendAsync(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                Console.WriteLine($"[Modrinth]: Upload for '{filePath.FullName}' successful");
                return true;
            }

            var task = response.Content.ReadAsStringAsync();
            task.Wait();
            Console.WriteLine(task.Result);
            Console.Error.WriteLine($"[Modrinth]: Error uploading file '{filePath.FullName}'");
            return false;
        }
        catch (Exception e)
        {
            Console.Error.WriteLine($"[Modrinth]: Error uploading file '{filePath.FullName}'");
            Console.WriteLine(e);
            return false;
        }
    }

    public static async Task<bool> UploadFileToCurse(CurseforgeInfo curseforgeInfo, CurseforgeMetaData metaData,
        FileInfo filePath)
    {
        try
        {
            var Client = new RestClient("https://minecraft.curseforge.com");
            var path = $"/api/projects/{curseforgeInfo.ModId}/upload-file";

            var metajson = JsonSerializer.Serialize(metaData, new JsonSerializerOptions
            {
                WriteIndented = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
            });
            var request = new RestRequest(path, Method.Post)
                .AddFile("file", filePath.FullName)
                .AddHeader("Accept", "application/json")
                .AddHeader("X-Api-Token", curseforgeInfo.Token)
                .AddParameter("metadata", metajson);
            request.AlwaysMultipartFormData = true;


            var response = await Client.PostAsync(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                Console.WriteLine($"[Curseforge]: Upload for '{filePath.FullName}' successful");
                return true;
            }

            Console.Error.WriteLine($"[Curseforge]: Error uploading file '{filePath.FullName}'");
            return false;
        }
        catch (Exception e)
        {
            Console.Error.WriteLine($"[Curseforge]: Error uploading file '{filePath.FullName}'");
            Console.WriteLine(e);
            return false;
        }
    }

    public static CurseforgeMetaData BuildCurseforgeMetaData(ModInfo modInfo, CurseforgeInfo curseforgeInfo,
        ModFileInfo modFile, string versionName)
    {
        var curseforgeVersionIds = GetCurseforgeVersionIds(curseforgeInfo, modFile);
        var curseforgeDependencies = modFile.ModLoader == Fabric
            ? curseforgeInfo.Projects.Select(dep => dep.Value).ToList()
            : new List<Project>();
        var curseforgeMetaData = new CurseforgeMetaData(
            versionName,
            curseforgeVersionIds.ToArray(),
            modInfo.Changelog,
            modInfo.ReleaseType,
            curseforgeDependencies);
        return curseforgeMetaData;
    }

    public static ModrinthMetaData BuildModrinthMetaData(ModInfo modInfo, ModrinthInfo modrinthInfo,
        ModFileInfo modFile, string versionName)
    {
        var gameVersions = new[] { modFile.MinecraftVersion };
        var modloaders = new[] { modFile.ModLoader };
        var modrinthDependencies = modFile.ModLoader == Fabric
            ? modrinthInfo.Dependencies.Select(dep => dep.Value).ToList()
            : new List<Dependency>();

        var modrinthMetaData = new ModrinthMetaData(
            versionName, modrinthInfo.ModId,
            modFile.ModVersion,
            gameVersions,
            modInfo.ReleaseType,
            modloaders,
            modInfo.Changelog,
            modFile.FileInfo.FullName,
            modrinthDependencies);
        return modrinthMetaData;
    }

    private static List<int> GetCurseforgeVersionIds(CurseforgeInfo curseforgeInfo, ModFileInfo modFile)
    {
        var versionIds = curseforgeInfo.CommonIds
            .Select(id => curseforgeInfo.VersionIds[id])
            .ToList();
        versionIds.Add(curseforgeInfo.VersionIds[modFile.ModLoader]);
        var idsForVersion = curseforgeInfo.IdsForVersion[modFile.MinecraftVersion]
            .Select(ids => curseforgeInfo.VersionIds[ids]).ToList();
        versionIds.AddRange(idsForVersion);

        // FIXME: Should not be needed but in some cases both modloaders ids get added 
        if (modFile.ModLoader == Fabric && versionIds.Contains(curseforgeInfo.VersionIds[Forge]))
            versionIds.Remove(curseforgeInfo.VersionIds[Forge]);

        if (modFile.ModLoader == Forge && versionIds.Contains(curseforgeInfo.VersionIds[Fabric]))
            versionIds.Remove(curseforgeInfo.VersionIds[Fabric]);

        return versionIds;
    }
}