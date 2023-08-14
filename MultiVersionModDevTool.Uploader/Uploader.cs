using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using de.z0rdak.moddev.util.Models.Platforms.Curseforge;
using de.z0rdak.moddev.util.Models.Platforms.Modrinth;
using RestSharp;

namespace de.z0rdak.moddev.util;

public static class Uploader
{
    public static async Task<bool> UploadFileToModrinth(ModrinthInfo modrinthInfo, ModrinthMetaData metaData,
        FileInfo filePath)
    {
        var metajson = JsonSerializer.Serialize(metaData, new JsonSerializerOptions
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
        });
        var client = new HttpClient();
        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.modrinth.com/v2/version");
        request.Headers.Add("User-Agent", "Z0rdak/Yet-Another-World-Protector");
        request.Headers.Add("Authorization", modrinthInfo.Token);
        var content = new MultipartFormDataContent();
        content.Add(new StringContent(metajson), "data");
        content.Add(new StreamContent(File.OpenRead(filePath.FullName)), "file", filePath.Name);
        request.Content = content;
        try
        {
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

        try
        {
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
}