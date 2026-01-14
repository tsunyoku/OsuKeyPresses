using System;
using System.IO;

namespace OsuKeyPresses.UI;

public static class Logger
{
    public static void LogInformation(string message)
        => Log("Info", message);
    
    public static void LogWarning(string message)
        => Log("Warn", message);
    
    public static void LogError(string message)
        => Log("Error", message);

    private static void Log(string level, string message)
    {
        var finalMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} - {level}] {message}{Environment.NewLine}";

        if (!File.Exists(GlobalConfig.LogFile))
            File.WriteAllText(GlobalConfig.LogFile, finalMessage);
        else
            File.AppendAllText(GlobalConfig.LogFile, finalMessage);
    }
}