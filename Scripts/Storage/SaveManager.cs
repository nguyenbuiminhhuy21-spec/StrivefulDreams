#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Code_Game.Domain;

namespace Code_Game.Scripts.Storage;

public static class SaveManager
{
    private static readonly string GameName = "ISV";
    private static readonly string SaveFolder = "Saves";
    private static readonly string SaveFileName = "save.json";
    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
    {
        WriteIndented = true
    };

    public static string GetSavePath()
    {
        var appData = GetOsAppDataFolder();
        var path = Path.Combine(appData, GameName, SaveFolder);

        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);

        return path;
    }

    public static string GetFarmPath(string farmName)
    {
        var savePath = GetSavePath();
        var safeFarmName = MakeSafeFileName(farmName);
        return Path.Combine(savePath, safeFarmName);
    }

    public static string GetFarmFilePath(string farmName)
    {
        return Path.Combine(GetFarmPath(farmName), SaveFileName);
    }

    public static void Save(SaveData data, string? farmName = null)
    {
        if (string.IsNullOrWhiteSpace(farmName))
            farmName = "DefaultFarm";

        data.LastSaved = DateTime.Now;
        data.GameVersion = "1.0.0";

        var farmPath = GetFarmPath(farmName);
        if (!Directory.Exists(farmPath))
            Directory.CreateDirectory(farmPath);

        var fullPath = GetFarmFilePath(farmName);
        var json = JsonSerializer.Serialize(data, JsonOptions);
        File.WriteAllText(fullPath, json);

        Console.WriteLine($"✅ Game saved to: {fullPath}");
    }

    public static SaveData? Load(string farmName)
    {
        var fullPath = GetFarmFilePath(farmName);
        if (!File.Exists(fullPath))
        {
            Console.WriteLine("❌ Save file not found!");
            return null;
        }

        var json = File.ReadAllText(fullPath);
        var data = JsonSerializer.Deserialize<SaveData>(json, JsonOptions);
        Console.WriteLine($"✅ Game loaded from: {fullPath}");
        return data;
    }

    public static bool FarmExists(string farmName)
    {
        return File.Exists(GetFarmFilePath(farmName));
    }

    public static void Delete(string farmName)
    {
        var farmPath = GetFarmPath(farmName);
        if (Directory.Exists(farmPath))
            Directory.Delete(farmPath, true);
    }

    public static IEnumerable<string> GetExistingFarmNames()
    {
        var savePath = GetSavePath();
        if (!Directory.Exists(savePath))
            return Array.Empty<string>();

        return Directory.GetDirectories(savePath)
            .Select(dirPath => new { FullPath = dirPath, FarmName = Path.GetFileName(dirPath) })
            .Where(item => !string.IsNullOrEmpty(item.FarmName) && File.Exists(Path.Combine(item.FullPath, SaveFileName)))
            .Select(item => item.FarmName)
            .OrderBy(name => name)
            .ToList()!;
    }

    private static string MakeSafeFileName(string input)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var safe = string.Concat(input.Where(ch => !invalidChars.Contains(ch))).Trim();
        return string.IsNullOrWhiteSpace(safe) ? "Farm" : safe;
    }

    private static string GetOsAppDataFolder()
    {
        if (OperatingSystem.IsWindows())
            return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        if (OperatingSystem.IsMacOS())
        {
            var home = Environment.GetEnvironmentVariable("HOME") ?? Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            return Path.Combine(home, "Library", "Application Support");
        }

        if (OperatingSystem.IsLinux())
        {
            var home = Environment.GetEnvironmentVariable("HOME") ?? Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            return Path.Combine(home, ".config");
        }

        return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
    }
}
