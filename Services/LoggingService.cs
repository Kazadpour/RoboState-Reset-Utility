using RoboStateResetUtility.Models;
using System.IO;

namespace RoboStateResetUtility.Services;

public class LoggingService
{
    private const string LogFilePath = "C:/temp/VPOS_Reset_Log.log";
    private readonly object _lockObject = new();

    public event EventHandler<LogEntry>? LogEntryAdded;

    public void Log(string message, string level = "INFO", string? details = null)
    {
        var entry = new LogEntry
        {
            Timestamp = DateTime.Now,
            Level = level,
            Message = message,
            Details = details
        };

        // Write to file
        lock (_lockObject)
        {
            try
            {
                File.AppendAllText(LogFilePath, entry.ToString() + Environment.NewLine);
            }
            catch (Exception ex)
            {
                // If we can't write to log file, at least raise the event
                System.Diagnostics.Debug.WriteLine($"Failed to write to log file: {ex.Message}");
            }
        }

        // Raise event for UI updates
        LogEntryAdded?.Invoke(this, entry);
    }

    public void LogInfo(string message, string? details = null)
    {
        Log(message, "INFO", details);
    }

    public void LogSuccess(string message, string? details = null)
    {
        Log(message, "SUCCESS", details);
    }

    public void LogWarning(string message, string? details = null)
    {
        Log(message, "WARNING", details);
    }

    public void LogError(string message, string? details = null)
    {
        Log(message, "ERROR", details);
    }

    public void LogSession(string message)
    {
        var separator = new string('=', 80);
        lock (_lockObject)
        {
            try
            {
                File.AppendAllText(LogFilePath, $"{Environment.NewLine}{separator}{Environment.NewLine}");
                File.AppendAllText(LogFilePath, $"{message}{Environment.NewLine}");
                File.AppendAllText(LogFilePath, $"{separator}{Environment.NewLine}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to write session log: {ex.Message}");
            }
        }
    }
}
