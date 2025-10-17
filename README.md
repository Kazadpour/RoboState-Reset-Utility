# VPOS State Reset Utility

A Windows desktop application for managing and resetting the VPOS state on London Drugs Fujitsu self-checkout systems across multiple store locations.

## Overview

This utility allows IT administrators to efficiently scan and reset the `ResetVPOSData` attribute in the `vpos_state.cfg` file across all store self-checkouts in the domain. When this value is reset, the Fujitsu robot will sync VPOS with CPOS D365 on the next startup, helping to resolve common self-checkout issues.

## Features

- **Network Scanning**: Automatically discovers all accessible self-checkout systems across stores 002-092
- **Batch Operations**: Select and reset multiple self-checkouts simultaneously
- **Real-time Status**: View the current `ResetVPOSData` value for each checkout before making changes
- **Automatic Backup**: Creates timestamped backups before modifying configuration files
- **Comprehensive Logging**: All operations are logged to `C:/temp/VPOS_Reset_Log.log` for auditing
- **Modern Dark UI**: Clean, professional interface with London Drugs and Fujitsu branding
- **Toggle Support**: Set `ResetVPOSData` to either 0 or 1 as needed
- **Progress Tracking**: Visual feedback during scanning and reset operations

## System Requirements

- **Operating System**: Windows 10/11 or Windows Server 2016+
- **.NET Runtime**: .NET 8.0 Desktop Runtime
- **Permissions**: Domain account with access to `\\ldXXXscoposNNN\c$` paths
- **Network**: Access to store network drives

## Installation

### Option 1: Run from Release (Recommended)

1. Download the latest release from the releases page
2. Extract the ZIP file to a folder of your choice
3. Ensure you have the [.NET 8.0 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/8.0) installed
4. Run `RoboStateResetUtility.exe`

### Option 2: Build from Source

1. **Prerequisites**:
   - Install [Visual Studio 2022](https://visualstudio.microsoft.com/) or [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
   - Ensure you have the ".NET desktop development" workload installed

2. **Clone or Download** the repository to your local machine

3. **Restore NuGet Packages**:
   ```bash
   dotnet restore
   ```

4. **Build the Project**:
   ```bash
   dotnet build --configuration Release
   ```

5. **Run the Application**:
   ```bash
   dotnet run
   ```

   Or navigate to `bin\Release\net8.0-windows\` and run `RoboStateResetUtility.exe`

### Option 3: Publish Self-Contained Executable

To create a standalone executable that doesn't require .NET runtime installation:

```bash
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

The executable will be in `bin\Release\net8.0-windows\win-x64\publish\`

## Usage

### 1. Launch the Application

Run `RoboStateResetUtility.exe`. The application will open with a dark-themed interface.

### 2. Scan the Network

Click the **"Scan Network"** button to discover all accessible self-checkouts across stores:

- The scan covers stores 002 through 092
- Each store checks for self-checkouts 031-038 and 041-048
- Inaccessible systems are automatically skipped
- Current `ResetVPOSData` values are displayed for accessible checkouts

**Note**: The initial scan may take several minutes depending on network speed and the number of accessible stores.

### 3. Select Checkouts to Reset

After scanning completes:

- Expand individual stores to see their self-checkouts
- Check the boxes next to checkouts you want to reset
- Use **"Select All"** to select all accessible checkouts
- Use **"Deselect All"** to clear selections
- You can also select entire stores by checking the store-level checkbox

### 4. Choose Reset Value

Use the **"Reset Value"** dropdown to select:
- **0**: Disables VPOS reset
- **1**: Enables VPOS reset (default)

### 5. Execute Reset

Click the **"Reset Selected"** button:

- A confirmation dialog will appear showing how many checkouts will be affected
- The application will:
  1. Create a backup of each configuration file (stored in the same directory with timestamp)
  2. Update the `ResetVPOSData` value
  3. Show real-time progress
  4. Display success/failure status for each checkout

### 6. Review Results

- Check the **Activity Log** panel on the right for operation details
- Green status = Success
- Red status = Failed
- Orange status = Warning or processing
- A summary dialog will show total successes and failures

### 7. View Full Logs

Click the **document icon** button (top right) to open the full log file at `C:/temp/VPOS_Reset_Log.log`.

## Network Path Structure

The application accesses self-checkouts using this pattern:

```
\\ld{STORE}scopos{CHECKOUT}\c$\Robot\Data\vpos_state.cfg
```

**Where**:
- `{STORE}` = Store number (002-092), formatted as 3 digits
- `{CHECKOUT}` = Checkout number (031-038, 041-048), formatted as 3 digits

**Examples**:
- `\\ld002scopos031\c$\Robot\Data\vpos_state.cfg`
- `\\ld045scopos042\c$\Robot\Data\vpos_state.cfg`

## Configuration File Format

The `vpos_state.cfg` file is an XML file with this structure:

```xml
<Config>
  <ResetVPOSData>0</ResetVPOSData>
  <RegisterNumber>T002031</RegisterNumber>
  <RetailServerUrl>https://...</RetailServerUrl>
  ...
</Config>
```

The application modifies only the `<ResetVPOSData>` element value.

## Backup Files

When a configuration file is modified, a backup is automatically created in the same directory:

**Format**: `vpos_state_backup_YYYYMMDD_HHMMSS.cfg`

**Example**: `vpos_state_backup_20250117_143052.cfg`

**Location**: `\\ldXXXscoposNNN\c$\Robot\Data\`

## Logging

All operations are logged to: `C:\temp\VPOS_Reset_Log.log`

**Log entries include**:
- Timestamp
- Operation type (INFO, SUCCESS, WARNING, ERROR)
- Detailed messages
- File paths and values changed
- Error details when operations fail

**Log format**:
```
[2025-01-17 14:30:52] [INFO] Application started
[2025-01-17 14:31:15] [SUCCESS] Updated ResetVPOSData: 0 → 1
[2025-01-17 14:31:15] [ERROR] Error updating config file: Access denied
```

## Troubleshooting

### "No accessible checkouts found"

**Possible causes**:
- Network connectivity issues
- Insufficient permissions to access `\\ldXXXscoposNNN\c$` paths
- Stores/checkouts are offline or not on the domain
- Firewall blocking SMB/CIFS traffic

**Solutions**:
- Verify you can manually access a test path (e.g., `\\ld002scopos031\c$`)
- Check your domain credentials have admin access
- Test network connectivity to store systems

### "Failed to update configuration file"

**Possible causes**:
- File is locked by another process (Fujitsu robot running)
- Insufficient write permissions
- Disk full on remote system

**Solutions**:
- Ensure the Fujitsu robot service is not running
- Verify write permissions on the remote path
- Check disk space on remote system

### "Configuration file not found"

**Possible causes**:
- Incorrect path structure
- File has been moved or deleted
- Robot software not installed

**Solutions**:
- Verify the file exists at: `\\ldXXXscoposNNN\c$\Robot\Data\vpos_state.cfg`
- Check if the Robot software is properly installed

### Application won't start

**Possible causes**:
- Missing .NET 8.0 Desktop Runtime
- Antivirus blocking execution

**Solutions**:
- Install [.NET 8.0 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/8.0)
- Add exception in antivirus software
- Run as administrator

## Security Considerations

- **Credentials**: The application uses your Windows domain credentials automatically
- **Access Control**: Ensure only authorized personnel have access to this tool
- **Audit Trail**: All operations are logged for compliance and troubleshooting
- **Backups**: Always created before modifications for rollback capability
- **Read-Only Scanning**: Network scanning performs read-only operations

## Support

For issues, questions, or feature requests:

1. Check the log file: `C:\temp\VPOS_Reset_Log.log`
2. Verify network connectivity and permissions
3. Review this README for troubleshooting steps
4. Contact your IT administrator or development team

## Technical Details

**Technology Stack**:
- .NET 8.0 WPF (Windows Presentation Foundation)
- MVVM Architecture Pattern
- Material Design UI Theme
- XML parsing with LINQ to XML
- Async/await for responsive UI

**Key Components**:
- `NetworkScanService`: Discovers and validates self-checkout systems
- `ConfigurationService`: Reads and writes XML configuration files
- `LoggingService`: Comprehensive operation logging
- `MainViewModel`: UI logic and command handling

## License

© 2025 London Drugs. Internal use only.

## Version History

### Version 1.0.0 (2025-01-17)
- Initial release
- Network scanning for stores 002-092
- Batch reset operations
- Automatic backups
- Comprehensive logging
- Modern dark theme UI
- Toggle support for reset values (0/1)
