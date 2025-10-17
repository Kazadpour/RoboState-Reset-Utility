using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RoboStateResetUtility.Models;
using RoboStateResetUtility.Services;

namespace RoboStateResetUtility.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly LoggingService _loggingService;
    private readonly NetworkScanService _networkScanService;
    private readonly ConfigurationService _configurationService;

    [ObservableProperty]
    private ObservableCollection<Store> _stores = new();

    [ObservableProperty]
    private ObservableCollection<string> _logMessages = new();

    [ObservableProperty]
    private bool _isScanning;

    [ObservableProperty]
    private bool _isProcessing;

    [ObservableProperty]
    private string _statusMessage = "Ready to scan network";

    [ObservableProperty]
    private int _progressValue;

    [ObservableProperty]
    private int _progressMaximum = 100;

    [ObservableProperty]
    private int _newResetValue = 1;

    // Expose a boolean toggle for the UI while preserving the int value for processing
    public bool ResetValueEnabled
    {
        get => NewResetValue == 1;
        set => NewResetValue = value ? 1 : 0;
    }

    partial void OnNewResetValueChanged(int value)
    {
        OnPropertyChanged(nameof(ResetValueEnabled));
    }

    [ObservableProperty]
    private string _storeNumber = "";

    public MainViewModel()
    {
        _loggingService = new LoggingService();
        _configurationService = new ConfigurationService(_loggingService);
        _networkScanService = new NetworkScanService(_loggingService, _configurationService);

        // Subscribe to log events
        _loggingService.LogEntryAdded += OnLogEntryAdded;

        _loggingService.LogInfo("Application started");
    }

    [RelayCommand]
    private async Task ScanNetworkAsync()
    {
        // Validate store number input
        if (string.IsNullOrWhiteSpace(StoreNumber))
        {
            MessageBox.Show("Please enter a store number.", "Store Number Required", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (!int.TryParse(StoreNumber, out int storeNum) || storeNum < 1 || storeNum > 999)
        {
            MessageBox.Show("Please enter a valid store number (1-999).", "Invalid Store Number", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        IsScanning = true;
        StatusMessage = $"Scanning Store {storeNum:D3}...";
        LogMessages.Clear();
        Stores.Clear();
        ProgressValue = 0;

        try
        {
            var progress = new Progress<string>(message =>
            {
                StatusMessage = message;
                AddLogMessage(message);
            });

            var foundStores = await _networkScanService.ScanStoreAsync(storeNum, progress);

            if (foundStores != null)
            {
                Stores.Add(foundStores);
                WireUpStore(foundStores);
            }

            StatusMessage = $"Scan complete: {GetTotalAccessibleCheckouts()} accessible checkout(s) found";
            AddLogMessage($"✓ Found {GetTotalAccessibleCheckouts()} accessible checkout(s) in Store {storeNum:D3}");
        }
        catch (Exception ex)
        {
            StatusMessage = "Scan failed";
            _loggingService.LogError("Network scan failed", ex.Message);
            MessageBox.Show($"An error occurred during scanning:\n{ex.Message}", "Scan Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsScanning = false;
            ProgressValue = 0;
        }
    }

    [RelayCommand]
    private async Task ResetSelectedAsync()
    {
        var selectedCheckouts = GetSelectedCheckouts();

        if (selectedCheckouts.Count == 0)
        {
            MessageBox.Show("Please select at least one self-checkout to reset.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var result = MessageBox.Show(
            $"Are you sure you want to set ResetVPOSData to {NewResetValue} for {selectedCheckouts.Count} selected checkout(s)?",
            "Confirm Reset",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question
        );

        if (result != MessageBoxResult.Yes)
        {
            return;
        }

        IsProcessing = true;
        ProgressMaximum = selectedCheckouts.Count;
        ProgressValue = 0;

        int successCount = 0;
        int failureCount = 0;

        _loggingService.LogSession($"Reset Operation Started - Value: {NewResetValue}");

        foreach (var checkout in selectedCheckouts)
        {
            StatusMessage = $"Processing {checkout.DisplayName}...";
            checkout.Status = "Processing...";
            checkout.StatusColor = "#FF9800";

            try
            {
                var success = await _configurationService.UpdateResetValueAsync(checkout.FullPath, NewResetValue);

                if (success)
                {
                    checkout.Status = $"Success - Set to {NewResetValue}";
                    checkout.StatusColor = "#4CAF50";
                    checkout.CurrentResetValue = NewResetValue;
                    successCount++;
                }
                else
                {
                    checkout.Status = "Failed";
                    checkout.StatusColor = "#F44336";
                    failureCount++;
                }
            }
            catch (Exception ex)
            {
                checkout.Status = "Error";
                checkout.StatusColor = "#F44336";
                failureCount++;
                _loggingService.LogError($"Error processing {checkout.DisplayName}", ex.Message);
            }

            ProgressValue++;
        }

        _loggingService.LogSuccess($"Reset operation completed: {successCount} successful, {failureCount} failed");

        StatusMessage = $"Complete: {successCount} successful, {failureCount} failed";
        IsProcessing = false;
        ProgressValue = 0;

        MessageBox.Show(
            $"Reset operation completed:\n\n✓ Successful: {successCount}\n✗ Failed: {failureCount}",
            "Operation Complete",
            MessageBoxButton.OK,
            MessageBoxImage.Information
        );
    }

    [RelayCommand]
    private void SelectAll()
    {
        foreach (var store in Stores)
        {
            store.IsSelected = true;
        }
        UpdateResetToggleFromSelection();
    }

    [RelayCommand]
    private void DeselectAll()
    {
        foreach (var store in Stores)
        {
            store.IsSelected = false;
        }
        UpdateResetToggleFromSelection();
    }

    [RelayCommand]
    private void ExpandAll()
    {
        foreach (var store in Stores)
        {
            store.IsExpanded = true;
        }
    }

    [RelayCommand]
    private void CollapseAll()
    {
        foreach (var store in Stores)
        {
            store.IsExpanded = false;
        }
    }

    [RelayCommand]
    private void OpenLogFile()
    {
        try
        {
            var logPath = "C:/temp/VPOS_Reset_Log.log";
            if (File.Exists(logPath))
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = logPath,
                    UseShellExecute = true
                });
            }
            else
            {
                MessageBox.Show("Log file does not exist yet.", "Log File", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error opening log file:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private List<SelfCheckout> GetSelectedCheckouts()
    {
        var selected = new List<SelfCheckout>();

        foreach (var store in Stores)
        {
            selected.AddRange(store.SelfCheckouts.Where(sco => sco.IsSelected && sco.IsAccessible));
        }

        return selected;
    }

    private int GetTotalAccessibleCheckouts()
    {
        return Stores.Sum(store => store.AccessibleCheckoutsCount);
    }

    private void OnLogEntryAdded(object? sender, LogEntry entry)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            var message = $"[{entry.Timestamp:HH:mm:ss}] {entry.Message}";
            AddLogMessage(message);
        });
    }

    private void AddLogMessage(string message)
    {
        LogMessages.Add(message);

        // Keep only last 100 messages to prevent memory issues
        while (LogMessages.Count > 100)
        {
            LogMessages.RemoveAt(0);
        }
    }

    private void WireUpStore(Store store)
    {
        foreach (var sco in store.SelfCheckouts)
        {
            sco.PropertyChanged += SelfCheckout_PropertyChanged;
        }

        if (store.SelfCheckouts is INotifyCollectionChanged incc)
        {
            incc.CollectionChanged += (s, e) =>
            {
                if (e.NewItems != null)
                {
                    foreach (var item in e.NewItems)
                    {
                        if (item is SelfCheckout sc)
                        {
                            sc.PropertyChanged += SelfCheckout_PropertyChanged;
                        }
                    }
                }
                if (e.OldItems != null)
                {
                    foreach (var item in e.OldItems)
                    {
                        if (item is SelfCheckout sc)
                        {
                            sc.PropertyChanged -= SelfCheckout_PropertyChanged;
                        }
                    }
                }
            };
        }
    }

    private void SelfCheckout_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SelfCheckout.IsSelected) || e.PropertyName == nameof(SelfCheckout.CurrentResetValue))
        {
            UpdateResetToggleFromSelection();
        }
    }

    private void UpdateResetToggleFromSelection()
    {
        var selected = GetSelectedCheckouts();
        if (selected.Count == 0)
        {
            return;
        }

        // If all selected have the same current value (0 or 1), reflect it.
        var values = selected
            .Select(s => s.CurrentResetValue)
            .Where(v => v.HasValue)
            .Select(v => v!.Value)
            .Distinct()
            .ToList();

        if (values.Count == 1 && (values[0] == 0 || values[0] == 1))
        {
            NewResetValue = values[0];
        }
    }
}
