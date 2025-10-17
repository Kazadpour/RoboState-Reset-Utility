using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using RoboStateResetUtility.Models;

namespace RoboStateResetUtility.Services;

public class NetworkScanService
{
    private readonly LoggingService _loggingService;
    private readonly ConfigurationService _configurationService;

    // Store range: 002 to 092
    private const int MinStoreNumber = 2;
    private const int MaxStoreNumber = 92;

    // Checkout ranges: 031-038 and 041-048
    private static readonly int[] CheckoutRanges = { 31, 32, 33, 34, 35, 36, 37, 38, 41, 42, 43, 44 };

    // Timeout used for probing network paths
    private static readonly TimeSpan PathCheckTimeout = TimeSpan.FromSeconds(2);

    public NetworkScanService(LoggingService loggingService, ConfigurationService configurationService)
    {
        _loggingService = loggingService;
        _configurationService = configurationService;
    }

    /// <summary>
    /// Scans all stores and their self-checkouts for accessibility
    /// </summary>
    public async Task<ObservableCollection<Store>> ScanNetworkAsync(IProgress<string>? progress = null)
    {
        _loggingService.LogSession($"Network Scan Started - {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        _loggingService.LogInfo("Starting network scan for stores and self-checkouts...");

        var stores = new ObservableCollection<Store>();
        int totalAccessible = 0;
        int totalScanned = 0;

        for (int storeNum = MinStoreNumber; storeNum <= MaxStoreNumber; storeNum++)
        {
            var storeNumber = storeNum.ToString("D3"); // Format as 3 digits (e.g., 002)
            progress?.Report($"Scanning Store {storeNumber}...");

            var store = new Store { StoreNumber = storeNumber };
            bool storeHasAccessibleCheckouts = false;

            foreach (var checkoutNum in CheckoutRanges)
            {
                totalScanned++;
                var checkoutNumber = checkoutNum.ToString("D3");
                var networkPath = $"\\\\ld{storeNumber}scopos{checkoutNumber}\\c$";

                var checkout = new SelfCheckout
                {
                    CheckoutNumber = checkoutNumber,
                    NetworkPath = networkPath
                };

                // Check if the path is accessible
                var isAccessible = await CheckNetworkPathAsync(networkPath);
                checkout.IsAccessible = isAccessible;

                if (isAccessible)
                {
                    // Try to read the current reset value
                    var currentValue = await _configurationService.ReadResetValueAsync(checkout.FullPath);
                    checkout.CurrentResetValue = currentValue;
                    checkout.Status = currentValue.HasValue ? $"Current: {currentValue.Value}" : "Config Error";
                    checkout.StatusColor = currentValue.HasValue ? "#4CAF50" : "#FF9800";

                    storeHasAccessibleCheckouts = true;
                    totalAccessible++;

                    _loggingService.LogInfo(
                        $"Found accessible checkout: Store {storeNumber}, SCO {checkoutNumber}",
                        $"Current ResetVPOSData value: {currentValue?.ToString() ?? "N/A"}"
                    );
                }
                else
                {
                    checkout.Status = "Not Accessible";
                    checkout.StatusColor = "#757575";
                }

                store.SelfCheckouts.Add(checkout);
            }

            // Only add store if it has at least one accessible checkout
            if (storeHasAccessibleCheckouts)
            {
                stores.Add(store);
                _loggingService.LogInfo($"Store {storeNumber}: {store.AccessibleCheckoutsCount} accessible checkout(s)");
            }
        }

        _loggingService.LogSuccess(
            $"Network scan completed: {totalAccessible} accessible checkouts found out of {totalScanned} scanned",
            $"Found {stores.Count} store(s) with accessible checkouts"
        );

        progress?.Report($"Scan complete: {totalAccessible} accessible checkouts found");

        return stores;
    }

    /// <summary>
    /// Scans a specific store for self-checkouts
    /// </summary>
    public async Task<Store?> ScanStoreAsync(int storeNumber, IProgress<string>? progress = null)
    {
        var storeNum = storeNumber.ToString("D3"); // Format as 3 digits (e.g., 002)
        _loggingService.LogSession($"Store Scan Started - Store {storeNum} - {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        _loggingService.LogInfo($"Starting scan for Store {storeNum}...");

        progress?.Report($"Scanning Store {storeNum}...");

        var store = new Store { StoreNumber = storeNum };
        int totalAccessible = 0;
        int totalScanned = 0;

        foreach (var checkoutNum in CheckoutRanges)
        {
            totalScanned++;
            var checkoutNumber = checkoutNum.ToString("D3");
            var networkPath = $"\\\\ld{storeNum}scopos{checkoutNumber}\\c$";

            progress?.Report($"Checking SCO {checkoutNumber}...");

            var checkout = new SelfCheckout
            {
                CheckoutNumber = checkoutNumber,
                NetworkPath = networkPath
            };

            // Check if the path is accessible with timeout
            var isAccessible = await CheckNetworkPathAsync(networkPath);
            checkout.IsAccessible = isAccessible;

            if (isAccessible)
            {
                // Try to read the current reset value
                var currentValue = await _configurationService.ReadResetValueAsync(checkout.FullPath);
                checkout.CurrentResetValue = currentValue;
                checkout.Status = currentValue.HasValue ? $"Current: {currentValue.Value}" : "Config Error";
                checkout.StatusColor = currentValue.HasValue ? "#4CAF50" : "#FF9800";

                totalAccessible++;

                _loggingService.LogInfo(
                    $"Found accessible checkout: Store {storeNum}, SCO {checkoutNumber}",
                    $"Current ResetVPOSData value: {currentValue?.ToString() ?? "N/A"}"
                );
            }
            else
            {
                checkout.Status = "Not Accessible";
                checkout.StatusColor = "#757575";
            }

            store.SelfCheckouts.Add(checkout);
        }

        _loggingService.LogSuccess(
            $"Store {storeNum} scan completed: {totalAccessible} accessible checkouts found out of {totalScanned} scanned"
        );

        progress?.Report($"Scan complete: {totalAccessible} accessible checkouts found");

        // Return the store even if no accessible checkouts (for user feedback)
        return store;
    }

    /// <summary>
    /// Checks if a network path is accessible, enforcing a timeout.
    /// </summary>
    private async Task<bool> CheckNetworkPathAsync(string networkPath)
    {
        try
        {
            var ioTask = Task.Run(() =>
            {
                try
                {
                    // Check if the network path exists and is accessible
                    if (!Directory.Exists(networkPath))
                    {
                        return false;
                    }

                    // Check if the Robot\Data directory exists
                    var dataPath = Path.Combine(networkPath, "Robot", "Data");
                    if (!Directory.Exists(dataPath))
                    {
                        return false;
                    }

                    // Check if the vpos_state.cfg file exists
                    var configPath = Path.Combine(dataPath, "vpos_state.cfg");
                    return File.Exists(configPath);
                }
                catch (UnauthorizedAccessException)
                {
                    // No access to this path
                    return false;
                }
                catch (IOException)
                {
                    // Network or I/O error
                    return false;
                }
                catch (Exception)
                {
                    // Any other error
                    return false;
                }
            });

            var completed = await Task.WhenAny(ioTask, Task.Delay(PathCheckTimeout));
            if (completed != ioTask)
            {
                // Timeout occurred
                _loggingService.LogWarning($"Timeout checking path: {networkPath}");
                return false;
            }

            return await ioTask;
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>
    /// Refreshes the status of a specific checkout
    /// </summary>
    public async Task RefreshCheckoutStatusAsync(SelfCheckout checkout)
    {
        var isAccessible = await CheckNetworkPathAsync(checkout.NetworkPath);
        checkout.IsAccessible = isAccessible;

        if (isAccessible)
        {
            var currentValue = await _configurationService.ReadResetValueAsync(checkout.FullPath);
            checkout.CurrentResetValue = currentValue;
            checkout.Status = currentValue.HasValue ? $"Current: {currentValue.Value}" : "Config Error";
            checkout.StatusColor = currentValue.HasValue ? "#4CAF50" : "#FF9800";
        }
        else
        {
            checkout.Status = "Not Accessible";
            checkout.StatusColor = "#757575";
            checkout.CurrentResetValue = null;
        }
    }
}
