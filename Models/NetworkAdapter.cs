using System;
using System.Linq;
using System.Management;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MACAddressTool.Services;
using Microsoft.Win32;

namespace MACAddressTool.Models
{
    /// <summary>
    /// Result of a MAC change operation.
    /// </summary>
    public class MacChangeResult
    {
        public bool Success { get; private set; }
        public string Message { get; private set; }

        private MacChangeResult() { }

        public static MacChangeResult Ok(string message = "MAC address changed successfully.")
            => new MacChangeResult { Success = true, Message = message };

        public static MacChangeResult Error(string message)
            => new MacChangeResult { Success = false, Message = message };
    }

    /// <summary>
    /// Wraps a Windows network adapter with MAC address management capabilities.
    /// </summary>
    public class NetworkAdapter : IDisposable
    {
        private const string REGISTRY_BASE_PATH =
            @"SYSTEM\CurrentControlSet\Control\Class\{4D36E972-E325-11CE-BFC1-08002BE10318}";
        private const string REGISTRY_MAC_VALUE = "NetworkAddress";

        private ManagementObject _wmiAdapter;
        private bool _disposed;

        #region Properties

        public string AdapterName { get; private set; }
        public string FriendlyName { get; private set; }
        public int DeviceNumber { get; private set; } = -1;
        public bool IsValid => _wmiAdapter != null && DeviceNumber >= 0;

        public string RegistryKeyPath =>
            $@"{REGISTRY_BASE_PATH}\{DeviceNumber:D4}";

        /// <summary>
        /// Gets the currently active MAC as reported by the OS.
        /// </summary>
        public string ActiveMac
        {
            get
            {
                try
                {
                    var nic = GetManagedInterface();
                    if (nic == null) return null;

                    byte[] bytes = nic.GetPhysicalAddress().GetAddressBytes();
                    if (bytes == null || bytes.Length != 6) return null;

                    return BitConverter.ToString(bytes).Replace("-", "").ToUpperInvariant();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error reading active MAC: {ex.Message}");
                    return null;
                }
            }
        }

        /// <summary>
        /// Gets the MAC address currently stored in the registry (the spoofed address).
        /// Returns null if no custom address is set.
        /// </summary>
        public string RegistryMac
        {
            get
            {
                try
                {
                    using (RegistryKey key = Registry.LocalMachine.OpenSubKey(RegistryKeyPath, false))
                    {
                        if (key == null) return null;
                        var value = key.GetValue(REGISTRY_MAC_VALUE);
                        return value?.ToString();
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error reading registry MAC: {ex.Message}");
                    return null;
                }
            }
        }

        /// <summary>
        /// Whether this adapter currently has a spoofed MAC in the registry.
        /// </summary>
        public bool IsSpoofed => !string.IsNullOrEmpty(RegistryMac);

        public string Status
        {
            get
            {
                try
                {
                    return GetManagedInterface()?.OperationalStatus.ToString() ?? "Unknown";
                }
                catch { return "Unknown"; }
            }
        }

        #endregion

        #region Constructor & Initialization

        public NetworkAdapter(NetworkInterface nic) : this(nic.Description) { }

        public NetworkAdapter(string adapterName)
        {
            AdapterName = adapterName ?? throw new ArgumentNullException(nameof(adapterName));
            Initialize();
        }

        private void Initialize()
        {
            // Query WMI — escape single quotes to prevent WQL injection
            string safeName = AdapterName.Replace("'", "\\'");
            string query = $"SELECT * FROM Win32_NetworkAdapter WHERE Name='{safeName}'";

            using (var searcher = new ManagementObjectSearcher(query))
            using (var results = searcher.Get())
            {
                _wmiAdapter = results.Cast<ManagementObject>().FirstOrDefault();
            }

            if (_wmiAdapter == null)
            {
                System.Diagnostics.Debug.WriteLine($"WMI adapter not found: {AdapterName}");
                return;
            }

            // Extract the device number from the WMI path
            try
            {
                var match = Regex.Match(_wmiAdapter.Path.RelativePath, @"""(\d+)""$");
                if (match.Success)
                {
                    DeviceNumber = int.Parse(match.Groups[1].Value);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Could not parse device number for: {AdapterName}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error parsing device number: {ex.Message}");
            }

            // Get the friendly name from Network Connections
            FriendlyName = NetworkInterface.GetAllNetworkInterfaces()
                .Where(i => i.Description == AdapterName)
                .Select(i => i.Name)
                .FirstOrDefault();
        }

        #endregion

        #region MAC Address Operations

        /// <summary>
        /// Sets a MAC address in the registry and restarts the adapter.
        /// Pass null or empty string to CLEAR the custom MAC (restore to hardware default).
        /// </summary>
        public async Task<MacChangeResult> SetMacAsync(string mac)
        {
            if (!IsValid)
                return MacChangeResult.Error("Adapter is not properly initialized.");

            bool clearing = string.IsNullOrEmpty(mac);

            // Validate if we're setting (not clearing)
            if (!clearing)
            {
                mac = MacAddressService.NormalizeMac(mac);

                if (!MacAddressService.IsValidMac(mac, requireLocallyAdministered: true))
                    return MacChangeResult.Error(
                        $"'{mac}' is not a valid locally-administered MAC address.\n" +
                        "The second character must be 2, 6, A, or E.");
            }

            bool adapterDisabled = false;

            try
            {
                // Open the registry key
                using (RegistryKey regKey = Registry.LocalMachine.OpenSubKey(RegistryKeyPath, true))
                {
                    if (regKey == null)
                        return MacChangeResult.Error(
                            "Could not open registry key. Are you running as administrator?");

                    // Sanity check: make sure the registry key matches this adapter
                    string regAdapterModel = regKey.GetValue("AdapterModel") as string;
                    string regDriverDesc = regKey.GetValue("DriverDesc") as string;

                    if (regAdapterModel != AdapterName && regDriverDesc != AdapterName)
                        return MacChangeResult.Error(
                            $"Registry mismatch. Expected '{AdapterName}', " +
                            $"found Model='{regAdapterModel}', Desc='{regDriverDesc}'.");

                    // Back up the original MAC before first change
                    string currentActive = ActiveMac;
                    if (!clearing && currentActive != null)
                    {
                        MacAddressService.BackupOriginalMac(AdapterName, currentActive);
                    }

                    // Disable the adapter
                    uint disableResult = (uint)_wmiAdapter.InvokeMethod("Disable", null);
                    if (disableResult != 0)
                        return MacChangeResult.Error(
                            $"Failed to disable adapter (error code {disableResult}).");

                    adapterDisabled = true;

                    // Set or clear the registry value
                    if (clearing)
                    {
                        // Delete the NetworkAddress value to restore hardware default
                        try { regKey.DeleteValue(REGISTRY_MAC_VALUE, false); }
                        catch { /* Value might not exist, that's fine */ }
                    }
                    else
                    {
                        regKey.SetValue(REGISTRY_MAC_VALUE, mac, RegistryValueKind.String);
                    }
                }

                // Wait a moment for the registry change to take effect
                await Task.Delay(200);

                // Re-enable the adapter
                uint enableResult = await ReenableAdapterAsync();
                if (enableResult != 0)
                    return MacChangeResult.Error(
                        $"MAC was changed but failed to re-enable adapter (error code {enableResult}). " +
                        "You may need to manually enable it in Device Manager.");

                adapterDisabled = false;

                // Wait for the adapter to come back online
                await Task.Delay(1500);

                // Clear backup tracking if we restored
                if (clearing)
                {
                    MacAddressService.ClearBackup(AdapterName);
                }

                string resultMac = ActiveMac;
                string formattedMac = MacAddressService.FormatMac(resultMac ?? "");

                return clearing
                    ? MacChangeResult.Ok($"Custom MAC cleared. Active MAC: {formattedMac}")
                    : MacChangeResult.Ok($"MAC address changed to: {formattedMac}");
            }
            catch (Exception ex)
            {
                return MacChangeResult.Error($"Error: {ex.Message}");
            }
            finally
            {
                // Emergency re-enable if something went wrong
                if (adapterDisabled)
                {
                    try
                    {
                        await ReenableAdapterAsync();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(
                            $"Emergency re-enable failed: {ex.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// Restores the original (hardware) MAC by clearing the registry value.
        /// </summary>
        public async Task<MacChangeResult> RestoreOriginalMacAsync()
        {
            return await SetMacAsync(null);
        }

        /// <summary>
        /// Gets the stored original MAC address (from before any changes were made).
        /// </summary>
        public string GetBackedUpOriginalMac()
        {
            return MacAddressService.GetOriginalMac(AdapterName);
        }

        #endregion

        #region Private Helpers

        private NetworkInterface GetManagedInterface()
        {
            return NetworkInterface.GetAllNetworkInterfaces()
                .FirstOrDefault(nic => nic.Description == AdapterName);
        }

        private async Task<uint> ReenableAdapterAsync()
        {
            // Retry up to 3 times
            for (int attempt = 0; attempt < 3; attempt++)
            {
                try
                {
                    uint result = (uint)_wmiAdapter.InvokeMethod("Enable", null);
                    if (result == 0) return 0;

                    await Task.Delay(1000);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"Re-enable attempt {attempt + 1} failed: {ex.Message}");
                    if (attempt < 2) await Task.Delay(1000);
                }
            }

            return 1; // Failed after retries
        }

        #endregion

        #region Display

        public override string ToString()
        {
            string display = AdapterName;
            if (!string.IsNullOrEmpty(FriendlyName))
                display += $" ({FriendlyName})";
            return display;
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            if (!_disposed)
            {
                _wmiAdapter?.Dispose();
                _disposed = true;
            }
        }

        #endregion
    }
}