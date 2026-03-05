using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Windows.Forms;
using MACAddressTool.Models;
using MACAddressTool.Services;

namespace MACAddressTool.UI
{
    public partial class MainForm : Form
    {
        private List<NetworkAdapter> _adapters = new List<NetworkAdapter>();

        public MainForm()
        {
            InitializeComponent();
            this.Load += MainForm_Load;
            this.FormClosing += MainForm_FormClosing;
        }

        #region Form Lifecycle

        private void MainForm_Load(object sender, EventArgs e)
        {
            LoadAdapters();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Dispose all adapter objects
            foreach (var adapter in _adapters)
            {
                adapter.Dispose();
            }
            _adapters.Clear();
        }

        #endregion

        #region Adapter Loading

        private void LoadAdapters()
        {
            // Dispose previous
            foreach (var a in _adapters) a.Dispose();
            _adapters.Clear();
            cboAdapters.Items.Clear();

            // Get all network interfaces with a valid 6-byte MAC
            var nics = NetworkInterface.GetAllNetworkInterfaces()
                .Where(nic =>
                {
                    try
                    {
                        byte[] bytes = nic.GetPhysicalAddress().GetAddressBytes();
                        return bytes != null && bytes.Length == 6 &&
                               MacAddressService.IsValidMac(
                                   BitConverter.ToString(bytes).Replace("-", ""),
                                   requireLocallyAdministered: false);
                    }
                    catch { return false; }
                })
                .OrderByDescending(nic => nic.Speed)
                .ToList();

            foreach (var nic in nics)
            {
                var adapter = new NetworkAdapter(nic);
                if (adapter.IsValid)
                {
                    _adapters.Add(adapter);
                    cboAdapters.Items.Add(adapter);
                }
                else
                {
                    adapter.Dispose();
                }
            }

            if (cboAdapters.Items.Count > 0)
            {
                cboAdapters.SelectedIndex = 0;
            }
            else
            {
                SetStatus("No compatible network adapters found.", Color.OrangeRed);
            }
        }

        #endregion

        #region UI Updates

        private void UpdateDisplay()
        {
            var adapter = GetSelectedAdapter();
            if (adapter == null)
            {
                txtActiveMac.Text = "";
                txtRegistryMac.Text = "";
                txtOriginalMac.Text = "";
                lblSpoofStatus.Text = "";
                lblStatusValue.Text = "—";
                return;
            }

            // Status
            lblStatusValue.Text = adapter.Status;
            lblStatusValue.ForeColor = adapter.Status == "Up"
                ? Color.Green : Color.Gray;

            // Active MAC
            string activeMac = adapter.ActiveMac;
            txtActiveMac.Text = MacAddressService.FormatMac(activeMac ?? "");

            // Registry MAC
            string regMac = adapter.RegistryMac;
            txtRegistryMac.Text = string.IsNullOrEmpty(regMac)
                ? "(not set — using hardware)"
                : MacAddressService.FormatMac(regMac);

            // Original MAC (from backup)
            string originalMac = adapter.GetBackedUpOriginalMac();
            txtOriginalMac.Text = string.IsNullOrEmpty(originalMac)
                ? "(same as active)"
                : MacAddressService.FormatMac(originalMac);

            // Spoof indicator
            if (adapter.IsSpoofed)
            {
                lblSpoofStatus.Text = "⚠ SPOOFED";
                lblSpoofStatus.ForeColor = Color.OrangeRed;
            }
            else
            {
                lblSpoofStatus.Text = "✓ Original";
                lblSpoofStatus.ForeColor = Color.Green;
            }

            // Update button states
            btnClear.Enabled = adapter.IsSpoofed;
            btnRestore.Enabled = !string.IsNullOrEmpty(originalMac);
        }

        private void ValidateNewMacInput()
        {
            string input = MacAddressService.NormalizeMac(txtNewMac.Text);

            if (string.IsNullOrEmpty(input))
            {
                lblValidation.Text = "";
                btnApply.Enabled = false;
                return;
            }

            if (input.Length < 12)
            {
                lblValidation.Text = $"({input.Length}/12 chars)";
                lblValidation.ForeColor = Color.Gray;
                btnApply.Enabled = false;
                return;
            }

            if (MacAddressService.IsValidMac(input, requireLocallyAdministered: true))
            {
                lblValidation.Text = "✓ Valid";
                lblValidation.ForeColor = Color.Green;
                btnApply.Enabled = true;
            }
            else
            {
                lblValidation.Text = "✗ Invalid (2nd char must be 2/6/A/E)";
                lblValidation.ForeColor = Color.Red;
                btnApply.Enabled = false;
            }
        }

        private NetworkAdapter GetSelectedAdapter()
        {
            return cboAdapters.SelectedItem as NetworkAdapter;
        }

        private void SetStatus(string message, Color? color = null)
        {
            statusLabel.Text = message;
            statusLabel.ForeColor = color ?? SystemColors.ControlText;
        }

        private void SetBusy(bool busy, string message = null)
        {
            progressBar.Visible = busy;
            grpChangeMac.Enabled = !busy;
            cboAdapters.Enabled = !busy;

            if (message != null)
                SetStatus(message);
        }

        #endregion

        #region Event Handlers

        private void cboAdapters_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateDisplay();
        }

        private void txtNewMac_TextChanged(object sender, EventArgs e)
        {
            ValidateNewMacInput();
        }

        private void btnGenerate_Click(object sender, EventArgs e)
        {
            txtNewMac.Text = MacAddressService.FormatMac(MacAddressService.GenerateRandomMac());
        }

        private async void btnApply_Click(object sender, EventArgs e)
        {
            var adapter = GetSelectedAdapter();
            if (adapter == null) return;

            string newMac = MacAddressService.NormalizeMac(txtNewMac.Text);

            if (!MacAddressService.IsValidMac(newMac))
            {
                MessageBox.Show(
                    "The entered MAC address is not valid.\n\n" +
                    "Requirements:\n" +
                    "• 12 hexadecimal characters\n" +
                    "• Second character must be 2, 6, A, or E (locally administered)\n" +
                    "• Cannot be all zeros or all F's",
                    "Invalid MAC Address",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Confirm
            var confirm = MessageBox.Show(
                $"Change MAC address of:\n\n" +
                $"  Adapter: {adapter.AdapterName}\n" +
                $"  Current: {MacAddressService.FormatMac(adapter.ActiveMac ?? "")}\n" +
                $"  New:     {MacAddressService.FormatMac(newMac)}\n\n" +
                "The adapter will be temporarily disabled.\n" +
                "Continue?",
                "Confirm MAC Change",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes) return;

            await ApplyMacChange(adapter, newMac);
        }

        private async void btnClear_Click(object sender, EventArgs e)
        {
            var adapter = GetSelectedAdapter();
            if (adapter == null) return;

            if (!adapter.IsSpoofed)
            {
                MessageBox.Show("No custom MAC is set.", "Nothing to Clear",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var confirm = MessageBox.Show(
                $"Clear the spoofed MAC address and restore hardware default?\n\n" +
                $"  Adapter: {adapter.AdapterName}\n" +
                $"  Current spoofed: {MacAddressService.FormatMac(adapter.RegistryMac ?? "")}\n\n" +
                "The adapter will be temporarily disabled.",
                "Confirm Clear",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes) return;

            await ApplyMacChange(adapter, null);
        }

        private async void btnRestore_Click(object sender, EventArgs e)
        {
            var adapter = GetSelectedAdapter();
            if (adapter == null) return;

            string originalMac = adapter.GetBackedUpOriginalMac();
            if (string.IsNullOrEmpty(originalMac))
            {
                MessageBox.Show(
                    "No original MAC backup found for this adapter.\n" +
                    "Try using 'Clear Spoof' instead to restore the hardware default.",
                    "No Backup Found",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var confirm = MessageBox.Show(
                $"Restore original MAC address?\n\n" +
                $"  Adapter: {adapter.AdapterName}\n" +
                $"  Current: {MacAddressService.FormatMac(adapter.ActiveMac ?? "")}\n" +
                $"  Original: {MacAddressService.FormatMac(originalMac)}\n\n" +
                "This will clear the registry entry and restore the hardware MAC.",
                "Confirm Restore",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes) return;

            // Restore = clear the registry value (hardware MAC takes over)
            await ApplyMacChange(adapter, null);
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            UpdateDisplay();
            SetStatus("Display refreshed.", Color.DarkGreen);
        }

        #endregion

        #region MAC Change Execution

        private async Task ApplyMacChange(NetworkAdapter adapter, string mac)
        {
            string action = string.IsNullOrEmpty(mac) ? "Clearing" : "Applying";
            SetBusy(true, $"{action} MAC address...");

            try
            {
                MacChangeResult result = await adapter.SetMacAsync(mac);

                if (result.Success)
                {
                    SetStatus(result.Message, Color.DarkGreen);
                    MessageBox.Show(result.Message, "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    SetStatus("Operation failed.", Color.Red);
                    MessageBox.Show(result.Message, "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                SetStatus("Unexpected error.", Color.Red);
                MessageBox.Show($"Unexpected error:\n\n{ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                SetBusy(false, statusLabel.Text);
                UpdateDisplay();
            }
        }

        #endregion
    }
}