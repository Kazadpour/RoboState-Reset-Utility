namespace RoboStateResetUtility.Models;

public class LogEntry
{
    public DateTime Timestamp { get; set; }
    public string Level { get; set; } = "INFO";
    public string Message { get; set; } = string.Empty;
    public string? Details { get; set; }

    public override string ToString()
    {
        var entry = $"[{Timestamp:yyyy-MM-dd HH:mm:ss}] [{Level}] {Message}";
        if (!string.IsNullOrEmpty(Details))
        {
            entry += $" - {Details}";
        }
        return entry;
    }
}
