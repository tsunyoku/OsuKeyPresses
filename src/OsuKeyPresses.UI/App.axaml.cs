using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using OsuLogger = osu.Framework.Logging.Logger;

namespace OsuKeyPresses.UI;

public partial class App : Application
{
    public override void Initialize()
    {
        GlobalConfig.Load();
        OsuLogger.Enabled = false;

        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;

        AvaloniaXamlLoader.Load(this);
    }
    
    private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        var exception = (Exception)e.ExceptionObject;
        Logger.LogError($"Unhandled exception:{Environment.NewLine}{exception}");
    }

    private static void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        Logger.LogError($"Unhandled exception:{Environment.NewLine}{e.Exception}");
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow();
        }

        base.OnFrameworkInitializationCompleted();
    }
}