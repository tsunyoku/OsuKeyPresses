using System;
using System.IO;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using OxyPlot;
using OxyPlot.Avalonia;
using OxyPlot.Axes;
using OxyPlot.Legends;
using Legend = OxyPlot.Legends.Legend;
using LinearAxis = OxyPlot.Axes.LinearAxis;
using LineSeries = OxyPlot.Series.LineSeries;

namespace ManiaKeyPresses.UI;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
        
        DragDrop.SetAllowDrop(this, true);
        AddHandler(DragDrop.DropEvent, Drop);
    }
    
    private MainViewModel ViewModel => (MainViewModel)DataContext!;
    
    private void OsuClientIdTextBox_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (sender is not TextBox textBox)
            return;
        
        GlobalConfig.UpdateOsuClientId(textBox.Text);
    }

    private void OsuClientSecretTextBox_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (sender is not TextBox textBox)
            return;
        
        GlobalConfig.UpdateOsuClientSecret(textBox.Text);
    }

    private async void LoadReplayButton_Click(object? sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(GlobalConfig.OsuClientId))
            return;

        if (string.IsNullOrWhiteSpace(GlobalConfig.OsuClientSecret))
            return;

        var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open replay",
            AllowMultiple = false,
            FileTypeFilter = [new FilePickerFileType("osu! replay") { Patterns = ["*.osr"] }]
        });

        if (!files.Any())
            return;

        var path = files.Single().TryGetLocalPath();

        if (Path.GetExtension(path) != ".osr")
            return;

        var replayPath = files.Single().TryGetLocalPath()!;
        AnalyseReplay(replayPath);
    }

    private void Drop(object? sender, DragEventArgs e)
    {
        var file = e.DataTransfer.TryGetFile();

        if (file is null)
            return;

        var path = file.TryGetLocalPath()!;

        if (Path.GetExtension(path) != ".osr")
            return;

        AnalyseReplay(path);
    }
    
    private void AnalyseReplay(string replayPath)
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
            Background = OxyColors.White
        };

        plotModel.Axes.Add(new LinearAxis
        {
            Position = AxisPosition.Bottom,
            Title = "pressing time (ms)",
            TitleFontSize = 15,
            Minimum = 0,
            Maximum = 160,
            MajorGridlineStyle = LineStyle.Solid,
            MajorGridlineColor = OxyColors.LightGray,
            IsZoomEnabled = false,
            IsPanEnabled = false,
        });
        
        plotModel.Axes.Add(new LinearAxis
        {
            Position = AxisPosition.Left,
            Title = "count",
            TitleFontSize = 15,
            MajorGridlineStyle = LineStyle.Solid,
            MajorGridlineColor = OxyColors.LightGray,
            IsZoomEnabled = false,
            IsPanEnabled = false,
        });
        
        plotModel.IsLegendVisible = true;
        
        plotModel.Legends.Add(new Legend
        {
            LegendPosition = LegendPosition.RightTop,
            LegendPlacement = LegendPlacement.Inside,
            LegendFontSize = 10
        });

        for (var i = 0; i < analysis.HoldTimes.Length; i++)
        {
            var lineSeries = new LineSeries
            {
                Title = $"key {i + 1}",
                Color = GetRainbowColor(i, analysis.HoldTimes.Length),
                StrokeThickness = 2,
                TrackerFormatString = "{0}\nHold Time: {2:0} ms\nCount: {4:0}",
            };

            for (var j = 0; j < analysis.HoldTimes[i].Length; j++)
            {
                lineSeries.Points.Add(new DataPoint(analysis.HoldTimes[i][j], analysis.HoldTimeCounts[i][j]));
            }
            
            plotModel.Series.Add(lineSeries);
        }

        ViewModel.UpdateReplay(replayPath);
        PlotView.Model = plotModel;
    }

    private static OxyColor GetRainbowColor(int index, int total)
    {
        const double saturation = 0.9;
        const double value = 0.9;

        const double c = value * saturation;
        const double m = value - c;
        
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
            (byte)((b + m) * 255)
        );
    }
}