using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace RoboStateResetUtility.Services;

public class ConfigurationService
{
    private readonly LoggingService _loggingService;

    public ConfigurationService(LoggingService loggingService)
    {
        _loggingService = loggingService;
    }

    /// <summary>
    /// Reads the current ResetVPOSData value from the configuration file
    /// </summary>
    public async Task<int?> ReadResetValueAsync(string configFilePath)
    {
        try
        {
            if (!File.Exists(configFilePath))
            {
                _loggingService.LogWarning($"Configuration file not found: {configFilePath}");
                return null;
            }

            var xmlContent = await File.ReadAllTextAsync(configFilePath);
            var doc = XDocument.Parse(xmlContent);

            var resetElement = doc.Descendants("ResetVPOSData").FirstOrDefault();
            if (resetElement != null && int.TryParse(resetElement.Value, out int value))
            {
                return value;
            }

            _loggingService.LogWarning($"ResetVPOSData element not found or invalid in: {configFilePath}");
            return null;
        }
        catch (Exception ex)
        {
            _loggingService.LogError($"Error reading config file: {configFilePath}", ex.Message);
            return null;
        }
    }

    /// <summary>
    /// Updates the ResetVPOSData value in the configuration file
    /// </summary>
    public async Task<bool> UpdateResetValueAsync(string configFilePath, int newValue)
    {
        try
        {
            if (!File.Exists(configFilePath))
            {
                _loggingService.LogError($"Configuration file not found: {configFilePath}");
                return false;
            }

            // Create backup first
            var backupPath = await CreateBackupAsync(configFilePath);
            if (backupPath == null)
            {
                _loggingService.LogError($"Failed to create backup for: {configFilePath}");
                return false;
            }

            // Read and parse XML
            var xmlContent = await File.ReadAllTextAsync(configFilePath);
            var doc = XDocument.Parse(xmlContent);

            // Find and update the ResetVPOSData element
            var resetElement = doc.Descendants("ResetVPOSData").FirstOrDefault();
            if (resetElement == null)
            {
                _loggingService.LogError($"ResetVPOSData element not found in: {configFilePath}");
                return false;
            }

            var oldValue = resetElement.Value;
            resetElement.Value = newValue.ToString();

            // Save the modified XML
            await File.WriteAllTextAsync(configFilePath, doc.ToString());

            _loggingService.LogSuccess(
                $"Updated ResetVPOSData: {oldValue} → {newValue}",
                $"File: {configFilePath}, Backup: {backupPath}"
            );

            return true;
        }
        catch (Exception ex)
        {
            _loggingService.LogError($"Error updating config file: {configFilePath}", ex.Message);
            return false;
        }
    }

    /// <summary>
    /// Creates a backup of the configuration file in the temp directory
    /// </summary>
    private async Task<string?> CreateBackupAsync(string configFilePath)
    {
        try
        {
            var directory = Path.GetDirectoryName(configFilePath);
            if (string.IsNullOrEmpty(directory))
            {
                return null;
            }

            var fileName = Path.GetFileNameWithoutExtension(configFilePath);
            var extension = Path.GetExtension(configFilePath);
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var backupFileName = $"{fileName}_backup_{timestamp}{extension}";
            var backupPath = Path.Combine(directory, backupFileName);

            await Task.Run(() => File.Copy(configFilePath, backupPath, overwrite: true));

            _loggingService.LogInfo($"Backup created: {backupPath}");
            return backupPath;
        }
        catch (Exception ex)
        {
            _loggingService.LogError($"Error creating backup for: {configFilePath}", ex.Message);
            return null;
        }
    }

    /// <summary>
    /// Validates that the configuration file has the expected structure
    /// </summary>
    public async Task<bool> ValidateConfigFileAsync(string configFilePath)
    {
        try
        {
            if (!File.Exists(configFilePath))
            {
                return false;
            }

            var xmlContent = await File.ReadAllTextAsync(configFilePath);
            var doc = XDocument.Parse(xmlContent);

            // Check for Config root element
            if (doc.Root?.Name != "Config")
            {
                return false;
            }

            // Check for ResetVPOSData element
            var resetElement = doc.Descendants("ResetVPOSData").FirstOrDefault();
            return resetElement != null;
        }
        catch
        {
            return false;
        }
    }
}
