using System;
using System.ComponentModel;
using System.IO;
using System.Text.Json;
using Avalonia.Styling;

namespace ManiaKeyPresses.UI;

public static class GlobalConfig
{
    private static readonly string ManiaKeyPressesFolder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "ManiaKeyPresses");

    public static readonly string BeatmapPath = Path.Combine(ManiaKeyPressesFolder, "beatmaps");

    private static readonly string ConfigFile = Path.Combine(ManiaKeyPressesFolder, "config.json");
    
    public static readonly string ScreenshotPath = Path.Combine(ManiaKeyPressesFolder, "screenshots");

    public static string? OsuClientId { get; private set; }

    public static string? OsuClientSecret { get; private set; }

    public static string Theme { get; private set; } = "Dark";
    
    public static bool IsDarkMode => Theme == "Dark";

    public static void Load()
    {
        if (!Directory.Exists(ManiaKeyPressesFolder))
            Directory.CreateDirectory(ManiaKeyPressesFolder);

        if (!File.Exists(ConfigFile))
            return;

        var config = JsonSerializer.Deserialize<Config>(File.ReadAllText(ConfigFile));

        if (config is null)
            return;

        OsuClientId = config.OsuClientId;
        OsuClientSecret = config.OsuClientSecret;

        if (config.ThemeName is not null)
            Theme = config.ThemeName;
    }

    public static void UpdateOsuClientId(string? clientId)
    {
        OsuClientId = clientId;

        NotifyPropertyChanged(nameof(OsuClientId));
        UpdateConfigFile();
    }
    
    public static void UpdateOsuClientSecret(string? clientSecret)
    {
        OsuClientSecret = clientSecret;
        
        NotifyPropertyChanged(nameof(OsuClientSecret));
        UpdateConfigFile();
    }

    public static void UpdateTheme(ThemeVariant theme)
    {
        Theme = (string)theme.Key;

        NotifyPropertyChanged(nameof(Theme));
        UpdateConfigFile();
    }

    private static void UpdateConfigFile()
    {
        var config = new Config
        {
            OsuClientId = OsuClientId,
            OsuClientSecret = OsuClientSecret,
            ThemeName = Theme,
        };

        File.WriteAllText(ConfigFile, JsonSerializer.Serialize(config));
    }
        
    public static event PropertyChangedEventHandler? PropertyChanged;
    
    private static void NotifyPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(null, new PropertyChangedEventArgs(propertyName));
    }
}

file class Config
{
    public string? OsuClientId { get; init; }
    
    public string? OsuClientSecret { get; init; }
    
    public string? ThemeName { get; init; }
}