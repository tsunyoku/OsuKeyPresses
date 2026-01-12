using System;
using System.IO;
using System.Linq;
using Avalonia.Controls;
using OxyPlot;
using OxyPlot.Avalonia;
using OxyPlot.Axes;
using OxyPlot.Legends;
using Legend = OxyPlot.Legends.Legend;
using LinearAxis = OxyPlot.Axes.LinearAxis;
using LineSeries = OxyPlot.Series.LineSeries;

namespace ManiaKeyPresses.UI;

public partial class AnalysisWindow : UserControl
{
    public AnalysisWindow()
    {
        InitializeComponent();
    }

    private MainViewModel ViewModel => (MainViewModel)DataContext!;
    
    public void AnalyseReplay(string replayPath)
    {
        if (string.IsNullOrWhiteSpace(replayPath))
            return;

        var analyser = new ManiaKeyPressAnalyser(
            replayPath,
            GlobalConfig.OsuClientId!,
            GlobalConfig.OsuClientSecret!,
            GlobalConfig.BeatmapPath);

        var analysis = analyser.AnalyseReplay();
        
        var plotModel = new PlotModel
        {
            Title = "Key Hold Time Distribution",
            Background = GlobalConfig.IsDarkMode ? OxyColors.Black : OxyColors.White,
            TextColor = GlobalConfig.IsDarkMode ? OxyColors.White : OxyColors.Black,
            TitleColor = GlobalConfig.IsDarkMode ? OxyColors.White : OxyColors.Black,
            PlotAreaBorderColor = GlobalConfig.IsDarkMode ? OxyColors.Gray : OxyColors.Black
        };

        var gridColour = GlobalConfig.IsDarkMode ? OxyColors.DarkGray : OxyColors.LightGray;
        var axisColour = GlobalConfig.IsDarkMode ? OxyColors.White : OxyColors.Black;

        plotModel.Axes.Add(new LinearAxis
        {
            Position = AxisPosition.Bottom,
            Title = "pressing time (ms)",
            TitleFontSize = 15,
            TitleColor = axisColour,
            TextColor = axisColour,
            AxislineColor = axisColour,
            TicklineColor = axisColour,
            Minimum = 0,
            Maximum = 160,
            MajorGridlineStyle = LineStyle.Solid,
            MajorGridlineColor = gridColour,
            MinorGridlineColor = gridColour,
            IsZoomEnabled = false,
            IsPanEnabled = false,
        });

        double maxHoldTimeCount = analysis.HoldTimeCounts.SelectMany(x => x).Max();
        
        plotModel.Axes.Add(new LinearAxis
        {
            Position = AxisPosition.Left,
            Title = "count",
            TitleFontSize = 15,
            TitleColor = axisColour,
            TextColor = axisColour,
            AxislineColor = axisColour,
            TicklineColor = axisColour,
            Maximum = maxHoldTimeCount + maxHoldTimeCount * 0.1,
            MajorGridlineStyle = LineStyle.Solid,
            MajorGridlineColor = gridColour,
            MinorGridlineColor = gridColour,
            IsZoomEnabled = false,
            IsPanEnabled = false,
        });
        
        plotModel.IsLegendVisible = true;
        
        plotModel.Legends.Add(new Legend
        {
            LegendPosition = LegendPosition.RightTop,
            LegendPlacement = LegendPlacement.Inside,
            LegendFontSize = 10,
            LegendTextColor = GlobalConfig.IsDarkMode ? OxyColors.White : OxyColors.Black,
            LegendBackground = OxyColors.Transparent,
            LegendBorder = GlobalConfig.IsDarkMode ? OxyColors.Gray : OxyColors.LightGray
        });

        for (var i = 0; i < analysis.HoldTimes.Length; i++)
        {
            var lineSeries = new LineSeries
            {
                Title = $"key {i + 1}",
                Color = GetRainbowColour(i, analysis.HoldTimes.Length, GlobalConfig.IsDarkMode),
                StrokeThickness = 2,
                TrackerFormatString = "{0}\nHold Time: {2:0} ms\nCount: {4:0}",
            };

            for (var j = 0; j < analysis.HoldTimes[i].Length; j++)
            {
                lineSeries.Points.Add(new DataPoint(analysis.HoldTimes[i][j], analysis.HoldTimeCounts[i][j]));
            }
            
            plotModel.Series.Add(lineSeries);
        }

        ViewModel.UpdateReplay(Path.GetFileName(replayPath));
        ViewModel.UpdateScoreInfo(analysis.Score.ScoreInfo);

        var osuApiClient = new OsuApiClient(GlobalConfig.OsuClientId!, GlobalConfig.OsuClientSecret!);

        var score = osuApiClient!.GetLegacyScore(
            analysis.Score.ScoreInfo.LegacyOnlineID,
            analysis.Score.ScoreInfo.Ruleset.ShortName);

        var user = osuApiClient.GetUser(score.UserId, analysis.Score.ScoreInfo.Ruleset.ShortName);

        ViewModel.UpdateUser(user);
        
        PlotView.Model = plotModel;
    }

    private static OxyColor GetRainbowColour(int index, int total, bool isDarkTheme)
    {
        var saturation = isDarkTheme ? 0.6 : 0.9;
        var value = isDarkTheme ? 0.85 : 0.8;

        var c = value * saturation;
        var m = value - c;
        
        // Create a hue using the index & total
        // Then do standard HSV->RGB conversion
        // https://en.wikipedia.org/wiki/HSL_and_HSV#HSV_to_RGB
        var hue = (index * 360.0 / total) % 360.0;

        var x = c * (1 - Math.Abs((hue / 60.0) % 2 - 1));
    
        double r, g, b;
    
        switch (hue)
        {
            case < 60:
                r = c;
                g = x;
                b = 0;
                break;
            case < 120:
                r = x;
                g = c;
                b = 0;
                break;
            case < 180:
                r = 0;
                g = c;
                b = x;
                break;
            case < 240:
                r = 0;
                g = x;
                b = c;
                break;
            case < 300:
                r = x;
                g = 0;
                b = c;
                break;
            default:
                r = c;
                g = 0;
                b = x;
                break;
        }
    
        return OxyColor.FromRgb(
            (byte)((r + m) * 255), 
            (byte)((g + m) * 255), 
            (byte)((b + m) * 255));
    }
}