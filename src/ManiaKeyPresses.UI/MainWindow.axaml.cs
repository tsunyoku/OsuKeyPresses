using System;
using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using Avalonia.Styling;
using Avalonia.VisualTree;
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

        if (Application.Current is not null)
        {
            Application.Current.RequestedThemeVariant = new ThemeVariant(GlobalConfig.Theme, null);
            ViewModel.UpdateIsDarkMode(GlobalConfig.Theme == "Dark");
        }
        
        DragDrop.SetAllowDrop(this, true);
        AddHandler(DragDrop.DropEvent, Drop);
        
        AnalysisControl.DataContext = DataContext;
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
        AnalysisControl.AnalyseReplay(replayPath);
    }

    private async void SaveScreenshotButton_Click(object? sender, RoutedEventArgs e)
    {
        using var bitmap = CaptureAnalysisControl();

        var screenshotFileName = $"keypresses_{ViewModel.ScoreInfo!.LegacyOnlineID}.png";

        if (!Directory.Exists(GlobalConfig.ScreenshotPath))
            Directory.CreateDirectory(GlobalConfig.ScreenshotPath);
        
        var path = Path.Combine(GlobalConfig.ScreenshotPath, screenshotFileName);
        bitmap.Save(path);

        var clipboard = GetTopLevel(this)?.Clipboard;

        if (clipboard is null)
            return;

        var file = await StorageProvider.TryGetFileFromPathAsync(path);
        await clipboard.SetFileAsync(file);
    }
    
    /// <summary>
    /// Captures the analysis control window by making a new, invisible window to render into a bitmap
    /// </summary>
    /// <returns></returns>
    private RenderTargetBitmap CaptureAnalysisControl()
    {
        var tempWindow = new Window
        {
            Width = AnalysisControl.Bounds.Width,
            Height = AnalysisControl.Bounds.Height,
            WindowStartupLocation = WindowStartupLocation.Manual,
            Position = new PixelPoint(-50000, -50000),
            ShowInTaskbar = false,
            SystemDecorations = SystemDecorations.None
        };
    
        var originalParent = (Panel)AnalysisControl.Parent;
        var originalIndex = originalParent.Children.IndexOf(AnalysisControl);

        originalParent.Children.Remove(AnalysisControl);
        tempWindow.Content = AnalysisControl;
        tempWindow.Show();

        var pixelSize = new PixelSize((int)tempWindow.ClientSize.Width, (int)tempWindow.ClientSize.Height);
        var bitmap = new RenderTargetBitmap(pixelSize);
        bitmap.Render(tempWindow);

        tempWindow.Content = null;
        originalParent.Children.Insert(originalIndex, AnalysisControl);
        tempWindow.Close();
    
        return bitmap;
    }
    
    private void ThemeToggleButton_Click(object sender, RoutedEventArgs e)
    {
        var app = Application.Current;

        if (app is null)
            return;

        var currentTheme = GlobalConfig.Theme;
        var newTheme = currentTheme == "Dark" ? ThemeVariant.Light : ThemeVariant.Dark;
        app.RequestedThemeVariant = newTheme;

        ViewModel.UpdateIsDarkMode(newTheme == ThemeVariant.Dark);
        GlobalConfig.UpdateTheme(newTheme);
    }

    private void Drop(object? sender, DragEventArgs e)
    {
        var file = e.DataTransfer.TryGetFile();

        if (file is null)
            return;

        var path = file.TryGetLocalPath()!;

        if (Path.GetExtension(path) != ".osr")
            return;

        AnalysisControl.AnalyseReplay(path);
    }
}