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
    }
}
