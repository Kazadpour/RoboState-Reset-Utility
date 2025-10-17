# Quick Start Guide

## First Time Setup

### 1. Install Prerequisites

Download and install the **.NET 8.0 Desktop Runtime**:
- Go to: https://dotnet.microsoft.com/download/dotnet/8.0
- Download "Desktop Runtime" for Windows x64
- Run the installer

### 2. Build the Application

Open a terminal in the project folder and run:

```bash
dotnet restore
dotnet build --configuration Release
```

The executable will be at: `bin\Release\net8.0-windows\RoboStateResetUtility.exe`

### 3. (Optional) Create Single Executable

To create a standalone .exe that doesn't need .NET installed:

```bash
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true
```

Find it at: `bin\Release\net8.0-windows\win-x64\publish\RoboStateResetUtility.exe`

## Quick Usage

1. **Run the app** - Double-click `RoboStateResetUtility.exe`
2. **Click "Scan Network"** - Discovers all accessible self-checkouts (may take a few minutes)
3. **Select checkouts** - Check the boxes for the ones you want to reset
4. **Choose value** - Select 0 or 1 from the "Reset Value" dropdown (usually 1)
5. **Click "Reset Selected"** - Confirm and wait for completion
6. **Check results** - View the Activity Log or open the full log file

## Troubleshooting

**Can't access stores?**
- Verify you're logged in with domain admin credentials
- Test manual access: `\\ld002scopos031\c$`

**App won't start?**
- Install .NET 8.0 Desktop Runtime
- Run as Administrator

**Reset failed?**
- Check if Fujitsu robot service is running (should be stopped)
- Verify write permissions on remote paths

## Files & Logs

- **Log File**: `C:\temp\VPOS_Reset_Log.log`
- **Backups**: Created in same folder as config (e.g., `vpos_state_backup_20250117_143052.cfg`)

## Project Structure

```
RoboState Reset Utility/
├── Models/              # Data models (Store, SelfCheckout, LogEntry)
├── Services/            # Business logic (Network, Config, Logging)
├── ViewModels/          # MVVM ViewModels
├── Views/               # UI (MainWindow)
├── Converters/          # XAML value converters
├── Styles/              # Custom UI styles
├── App.xaml[.cs]        # Application entry point
├── README.md            # Full documentation
└── *.csproj             # Project file
```

## Support

See [README.md](README.md) for full documentation.
