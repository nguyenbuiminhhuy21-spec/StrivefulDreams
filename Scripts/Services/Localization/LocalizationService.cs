using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Code_Game.Scripts.Constants;
using Code_Game.Scripts.Constants.Paths;

namespace Code_Game.Scripts.Services.Localization;

public class LocalizationService
{
    private static LocalizationService _instance;
    public static LocalizationService Instance => _instance ??= new LocalizationService();

    private Dictionary<string, string> _translations = new();
    private string _currentLocale = Locales.Vietnamese;
    private readonly string _localesPath;

    private LocalizationService()
    {
        _localesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, FolderNames.Data, FolderNames.Locales);
        
        // Fallback for dev environment
        if (!Directory.Exists(_localesPath))
        {
            _localesPath = Path.Combine(Directory.GetCurrentDirectory(), FolderNames.Data, FolderNames.Locales);
        }

        LoadLocale(_currentLocale);
    }

    public string CurrentLocale => _currentLocale;

    public void LoadLocale(string locale)
    {
        string filePath = Path.Combine(_localesPath, $"{locale}.json");
        
        if (File.Exists(filePath))
        {
            try
            {
                string json = File.ReadAllText(filePath);
                _translations = JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? new();
                _currentLocale = locale;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error] Failed to load locale {locale}: {ex.Message}");
            }
        }
        else
        {
            Console.WriteLine($"[Warning] Locale file not found: {filePath}");
        }
    }

    public string Get(string key)
    {
        if (_translations.TryGetValue(key, out string value))
        {
            return value;
        }
        return $"[{key}]"; // Fallback to key if not found
    }



    public void SetLanguage(string locale)
    {
        LoadLocale(locale);
    }
}
