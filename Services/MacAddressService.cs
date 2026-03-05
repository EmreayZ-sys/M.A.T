using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace MACAddressTool.Services
{
    /// <summary>
    /// Handles MAC address generation, validation, and original MAC backup/restore.
    /// </summary>
    public static class MacAddressService
    {
        private static readonly string BackupFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "MACAddressTool",
            "original_macs.json"
        );

        #region MAC Generation

        /// <summary>
        /// Generates a cryptographically random locally-administered unicast MAC address.
        /// </summary>
        public static string GenerateRandomMac()
        {
            byte[] bytes = new byte[6];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }

            // Set the locally-administered bit (bit 1 of first byte)
            bytes[0] = (byte)(bytes[0] | 0x02);
            // Clear the multicast bit (bit 0 of first byte) - must be unicast
            bytes[0] = (byte)(bytes[0] & 0xFE);

            return BitConverter.ToString(bytes).Replace("-", "").ToUpperInvariant();
        }

        #endregion

        #region Validation

        /// <summary>
        /// Validates a MAC address string.
        /// </summary>
        public static bool IsValidMac(string mac, bool requireLocallyAdministered = true)
        {
            if (string.IsNullOrWhiteSpace(mac))
                return false;

            mac = mac.Replace("-", "").Replace(":", "").Replace(".", "").ToUpperInvariant();

            if (mac.Length != 12)
                return false;

            if (!Regex.IsMatch(mac, "^[0-9A-F]{12}$"))
                return false;

            if (mac == "000000000000" || mac == "FFFFFFFFFFFF")
                return false;

            if (requireLocallyAdministered)
            {
                char c = mac[1];
                return c == '2' || c == '6' || c == 'A' || c == 'E';
            }

            return true;
        }

        /// <summary>
        /// Cleans and normalizes a MAC input string to 12 uppercase hex chars.
        /// </summary>
        public static string NormalizeMac(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            return input.Replace("-", "")
                        .Replace(":", "")
                        .Replace(".", "")
                        .Replace(" ", "")
                        .ToUpperInvariant();
        }

        /// <summary>
        /// Formats a 12-char MAC string with dashes: XX-XX-XX-XX-XX-XX
        /// </summary>
        public static string FormatMac(string mac)
        {
            if (string.IsNullOrWhiteSpace(mac) || mac.Length != 12)
                return mac ?? "";

            return $"{mac[0..2]}-{mac[2..4]}-{mac[4..6]}-{mac[6..8]}-{mac[8..10]}-{mac[10..12]}";
        }

        #endregion

        #region Original MAC Backup and Restore

        /// <summary>
        /// Saves the original MAC for an adapter so it can be restored later.
        /// Will NOT overwrite an existing backup (preserves the true original).
        /// </summary>
        public static void BackupOriginalMac(string adapterName, string mac)
        {
            if (string.IsNullOrWhiteSpace(adapterName) || string.IsNullOrWhiteSpace(mac))
                return;

            var backups = LoadBackups();

            if (!backups.ContainsKey(adapterName))
            {
                backups[adapterName] = mac;
                SaveBackups(backups);
            }
        }

        /// <summary>
        /// Gets the backed-up original MAC for an adapter, or null if none exists.
        /// </summary>
        public static string GetOriginalMac(string adapterName)
        {
            if (string.IsNullOrWhiteSpace(adapterName))
                return null;

            var backups = LoadBackups();
            return backups.TryGetValue(adapterName, out string mac) ? mac : null;
        }

        /// <summary>
        /// Removes the backup entry for an adapter (called after successful restore).
        /// </summary>
        public static void ClearBackup(string adapterName)
        {
            if (string.IsNullOrWhiteSpace(adapterName))
                return;

            var backups = LoadBackups();
            if (backups.Remove(adapterName))
            {
                SaveBackups(backups);
            }
        }

        private static Dictionary<string, string> LoadBackups()
        {
            try
            {
                if (!File.Exists(BackupFilePath))
                    return new Dictionary<string, string>();

                string json = File.ReadAllText(BackupFilePath);
                return JsonSerializer.Deserialize<Dictionary<string, string>>(json)
                       ?? new Dictionary<string, string>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load MAC backups: {ex.Message}");
                return new Dictionary<string, string>();
            }
        }

        private static void SaveBackups(Dictionary<string, string> backups)
        {
            try
            {
                string dir = Path.GetDirectoryName(BackupFilePath)!;
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(backups, options);
                File.WriteAllText(BackupFilePath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to save MAC backups: {ex.Message}");
            }
        }

        #endregion
    }
}