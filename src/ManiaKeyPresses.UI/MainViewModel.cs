using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using ManiaKeyPresses.Models;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;

namespace ManiaKeyPresses.UI;

public class MainViewModel : INotifyPropertyChanged
{
    public MainViewModel()
    {
        GlobalConfig.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName is nameof(GlobalConfig.OsuClientId) or nameof(GlobalConfig.OsuClientSecret))
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsOsuConfigured)));
            }
        };
    }

    public bool IsOsuConfigured => !string.IsNullOrEmpty(GlobalConfig.OsuClientId) &&
                                   !string.IsNullOrEmpty(GlobalConfig.OsuClientSecret);
    
    public string? CurrentReplayFileName { get; private set; }

    public bool HasReplay => !string.IsNullOrWhiteSpace(CurrentReplayFileName);

    public bool IsDarkMode { get; private set; }
    
    public ScoreInfo? ScoreInfo { get; private set; }

    public double? ScoreAccuracyPercentage => ScoreInfo?.Accuracy * 100;

    public int? PerfectCount => ScoreInfo?.Statistics.GetValueOrDefault(HitResult.Perfect);

    public int? GreatCount => ScoreInfo?.Statistics.GetValueOrDefault(HitResult.Great);

    public int? GoodCount => ScoreInfo?.Statistics.GetValueOrDefault(HitResult.Good);

    public int? OkCount => ScoreInfo?.Statistics.GetValueOrDefault(HitResult.Ok);
    
    public int? MehCount => ScoreInfo?.Statistics.GetValueOrDefault(HitResult.Meh);

    public int? MissCount => ScoreInfo?.Statistics.GetValueOrDefault(HitResult.Miss);

    public Bitmap[] ModImagePaths => ScoreInfo?.APIMods
        .Where(x => x.Acronym != "CL")
        .Select(x => new Bitmap(AssetLoader.Open(new Uri($"avares://ManiaKeyPresses.UI/Assets/Mods/{x.Acronym}.png"))))
        .ToArray() ?? [];
    
    public User? User { get; private set; }

    public string? UserUrl => User is not null ? $"https://osu.ppy.sh/users/{User.Id}" : null;

    public string? UserAvatarUrl => User is not null ? $"https://a.ppy.sh/{User.Id}" : null;

    public bool PlayerInformationAvailable => User is not null;

    public void UpdateReplay(string? replayFileName)
    {
        CurrentReplayFileName = replayFileName;

        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentReplayFileName)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HasReplay)));
    }

    public void UpdateScoreInfo(ScoreInfo? scoreInfo)
    {
        ScoreInfo = scoreInfo;
        
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ScoreInfo)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ScoreAccuracyPercentage)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PerfectCount)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(GreatCount)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(GoodCount)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(OkCount)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MehCount)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MissCount)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ModImagePaths)));
    }

    public void UpdateUser(User? user)
    {
        User = user;
        
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(User)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(UserUrl)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(UserAvatarUrl)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PlayerInformationAvailable)));
    }

    public void UpdateIsDarkMode(bool isDarkMode)
    {
        IsDarkMode = isDarkMode;
        
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsDarkMode)));
    }

    public event PropertyChangedEventHandler? PropertyChanged;
}