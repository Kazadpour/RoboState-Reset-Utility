using System;
using System.IO;
using System.Windows;

namespace RoboStateResetUtility;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Ensure log directory exists
        var logDir = Path.GetDirectoryName("C:/temp/VPOS_Reset_Log.log");
        if (!string.IsNullOrEmpty(logDir) && !Directory.Exists(logDir))
        {
            Directory.CreateDirectory(logDir);
        }

        // Global exception handlers to diagnose startup/runtime crashes
        this.DispatcherUnhandledException += (s, exArgs) =>
        {
            try
            {
                File.AppendAllText("C:/temp/VPOS_Reset_Log.log",
                    $"[FATAL {DateTime.Now:yyyy-MM-dd HH:mm:ss}] Unhandled UI exception: {exArgs.Exception}\n");
            }
            catch { /* ignore logging failure */ }
            MessageBox.Show($"An unexpected error occurred and was logged.\n{exArgs.Exception.Message}",
                "Application Error", MessageBoxButton.OK, MessageBoxImage.Error);
            exArgs.Handled = true;
        };

        AppDomain.CurrentDomain.UnhandledException += (s, exArgs) =>
        {
            try
            {
                var ex = exArgs.ExceptionObject as Exception;
                File.AppendAllText("C:/temp/VPOS_Reset_Log.log",
                    $"[FATAL {DateTime.Now:yyyy-MM-dd HH:mm:ss}] Unhandled domain exception: {ex}\n");
            }
            catch { /* ignore logging failure */ }
        };
    }
}
