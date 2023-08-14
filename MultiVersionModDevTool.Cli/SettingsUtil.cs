using System;
using System.Configuration;
using de.z0rdak.moddev.util.Models;
using de.z0rdak.moddev.util.Models.Environment;
using de.z0rdak.moddev.util.Models.Platforms;
using Microsoft.Extensions.Configuration;

namespace de.z0rdak.moddev.util;

public class SettingsUtil
{
    public static ModEnvironment LoadEnvironmentSettings(string basePath, string appsettings = "appsettings.json")
    {
        try
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile(appsettings, false)
                .Build();

            var modEnv = new ModEnvironment();
            var modEnvSection = configuration.GetRequiredSection("Environment");
            modEnvSection.Bind(modEnv);
            return modEnv;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex);
            throw new ConfigurationErrorsException("Unable to load 'Environment' configuration from appsettings.json");
        }
    }

    public static ModInfo LoadModInfoSettings(string basePath, string appsettings = "appsettings.json")
    {
        try
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile(appsettings, false)
                .Build();

            var modInfo = new ModInfo();
            var modInfoSection = configuration.GetRequiredSection("ModInfo");
            modInfoSection.Bind(modInfo);
            return modInfo;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex);
            throw new ConfigurationErrorsException("Unable to load 'ModInfo' configuration from appsettings.json");
        }
    }

    public static Platforms LoadPlatformSettings(string basePath, string appsettings = "appsettings.json")
    {
        try
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile(appsettings, false)
                .Build();

            var platforms = new Platforms();
            var platformSection = configuration.GetRequiredSection("Platforms");
            platformSection.Bind(platforms);
            return platforms;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex);
            throw new ConfigurationErrorsException("Unable to load 'Platforms' configuration from appsettings.json");
        }
    }
}