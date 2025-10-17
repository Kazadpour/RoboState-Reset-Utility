# VPOS State Reset Utility - Deployment Guide

## Package Information

**Version:** 1.0
**Build Date:** October 2025
**Package Name:** `VPOS-State-Reset-Utility-v1.0.zip`

## Deployment Package Contents

The deployment package includes:
- `RoboStateResetUtility.exe` - Main application executable
- `RoboStateResetUtility.dll` - Application library
- `CommunityToolkit.Mvvm.dll` - MVVM framework dependency
- `RoboStateResetUtility.deps.json` - Dependency configuration
- `RoboStateResetUtility.runtimeconfig.json` - Runtime configuration
- `README.txt` - User instructions

## Prerequisites

### Required Software
- **Operating System:** Windows 10 or Windows 11 (64-bit)
- **.NET Runtime:** .NET 8.0 Desktop Runtime or later
  - Download: https://dotnet.microsoft.com/download/dotnet/8.0
  - Select: ".NET Desktop Runtime" for Windows x64

### Network Requirements
- Access to the store network where self-checkout systems are located
- Proper network permissions to communicate with self-checkout devices

## Deployment Steps

### For IT Administrators

1. **Download the Package**
   - Obtain `VPOS-State-Reset-Utility-v1.0.zip` from the distribution location

2. **Extract the Files**
   ```
   Extract all files to a target directory, for example:
   C:\Program Files\VPOS State Reset Utility\
   ```

3. **Verify .NET Runtime**
   - Ensure .NET 8.0 Desktop Runtime is installed on target machines
   - Run `dotnet --list-runtimes` in PowerShell to verify
   - Look for: `Microsoft.WindowsDesktop.App 8.0.x`

4. **Create Desktop Shortcut (Optional)**
   - Right-click `RoboStateResetUtility.exe`
   - Select "Create Shortcut"
   - Move shortcut to Desktop or Start Menu

5. **Test the Installation**
   - Double-click `RoboStateResetUtility.exe`
   - Verify the application launches successfully
   - Check the modern UI appears correctly

### For End Users

1. **Extract the ZIP File**
   - Right-click on `VPOS-State-Reset-Utility-v1.0.zip`
   - Select "Extract All..."
   - Choose destination folder
   - Click "Extract"

2. **Run the Application**
   - Navigate to the extracted folder
   - Double-click `RoboStateResetUtility.exe`
   - The application will open with the updated modern interface

3. **First Time Setup**
   - No additional configuration required
   - Application is ready to use immediately

## Application Features

### Modern UI Design
- Clean, minimalist interface with light theme
- Rounded corners and subtle shadows
- Clear typography and visual hierarchy
- Responsive layout

### Core Functionality
- **Store Scanning:** Enter store number and scan for self-checkout systems
- **Batch Selection:** Select multiple checkouts for bulk operations
- **Reset Operations:** Perform state reset on selected devices
- **Activity Logging:** Real-time operation monitoring
- **Progress Tracking:** Visual feedback during operations

## Troubleshooting

### Application Won't Start
**Issue:** Double-clicking the .exe does nothing or shows an error

**Solutions:**
1. Verify .NET 8.0 Runtime is installed
2. Check Windows Event Viewer for error details
3. Ensure all files from ZIP are in the same directory
4. Run as Administrator (right-click > Run as Administrator)

### "Missing .NET Runtime" Error
**Solution:** Install .NET 8.0 Desktop Runtime from:
https://dotnet.microsoft.com/download/dotnet/8.0

### Network/Scanning Issues
**Issue:** Unable to find self-checkout systems

**Solutions:**
1. Verify network connectivity to store systems
2. Check firewall settings aren't blocking the application
3. Ensure correct store number is entered
4. Confirm network permissions are adequate

### Application Crashes
**Solutions:**
1. Check the log files in the application directory
2. Ensure Windows is up to date
3. Verify .NET Runtime version is 8.0 or higher
4. Try running in compatibility mode (Windows 10)

## Security Considerations

- Application requires network access to self-checkout systems
- Ensure proper user permissions before deploying
- Log files contain operation history - review retention policies
- Consider restricting access to authorized personnel only

## Distribution Methods

### Option 1: Network Share
```
\\fileserver\IT\Applications\VPOS-State-Reset-Utility-v1.0.zip
```

### Option 2: USB Drive
- Copy ZIP file to USB drive
- Distribute to target machines
- Extract and run

### Option 3: System Management Tools
- Deploy via SCCM, Intune, or similar tools
- Extract to standard location
- Create Start Menu shortcuts automatically

## Update/Upgrade Process

To update to a newer version:
1. Close any running instances of the application
2. Extract new version to the same directory (overwrite existing files)
3. Launch the updated application
4. Previous log files and settings are preserved

## Uninstallation

To remove the application:
1. Close any running instances
2. Delete the application folder
3. Remove any shortcuts created
4. (Optional) Remove log files if no longer needed

## Support Information

For technical support:
- Contact: IT Department
- Include: Application logs, error messages, and steps to reproduce issues

## Version History

**v1.0 (October 2025)**
- Modern minimalist UI redesign
- Light theme with clean aesthetics
- Improved typography and spacing
- Enhanced user experience
- All core functionality maintained

## License & Copyright

VPOS State Reset Utility
London Drugs x Fujitsu Self-Checkout Management
For internal use only

---

*Document Version: 1.0*
*Last Updated: October 17, 2025*
