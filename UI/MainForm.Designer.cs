namespace MACAddressTool.UI
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            // === Controls ===
            this.grpAdapter = new System.Windows.Forms.GroupBox();
            this.cboAdapters = new System.Windows.Forms.ComboBox();
            this.lblStatus = new System.Windows.Forms.Label();
            this.lblStatusValue = new System.Windows.Forms.Label();

            this.grpCurrentMac = new System.Windows.Forms.GroupBox();
            this.lblActiveMac = new System.Windows.Forms.Label();
            this.txtActiveMac = new System.Windows.Forms.TextBox();
            this.lblRegistryMac = new System.Windows.Forms.Label();
            this.txtRegistryMac = new System.Windows.Forms.TextBox();
            this.lblOriginalMac = new System.Windows.Forms.Label();
            this.txtOriginalMac = new System.Windows.Forms.TextBox();
            this.lblSpoofStatus = new System.Windows.Forms.Label();

            this.grpChangeMac = new System.Windows.Forms.GroupBox();
            this.lblNewMac = new System.Windows.Forms.Label();
            this.txtNewMac = new System.Windows.Forms.TextBox();
            this.lblValidation = new System.Windows.Forms.Label();
            this.btnGenerate = new System.Windows.Forms.Button();
            this.btnApply = new System.Windows.Forms.Button();
            this.btnClear = new System.Windows.Forms.Button();
            this.btnRestore = new System.Windows.Forms.Button();
            this.btnRefresh = new System.Windows.Forms.Button();

            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.statusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.progressBar = new System.Windows.Forms.ToolStripProgressBar();

            // === Layout ===
            this.SuspendLayout();
            this.grpAdapter.SuspendLayout();
            this.grpCurrentMac.SuspendLayout();
            this.grpChangeMac.SuspendLayout();
            this.statusStrip.SuspendLayout();

            // --- Form ---
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(520, 480);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "MAC Address Tool";
            this.Font = new System.Drawing.Font("Segoe UI", 9F);

            // --- Adapter Group ---
            this.grpAdapter.Text = "Network Adapter";
            this.grpAdapter.Location = new System.Drawing.Point(12, 12);
            this.grpAdapter.Size = new System.Drawing.Size(496, 75);

            this.cboAdapters.Location = new System.Drawing.Point(12, 25);
            this.cboAdapters.Size = new System.Drawing.Size(472, 23);
            this.cboAdapters.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboAdapters.SelectedIndexChanged += new System.EventHandler(this.cboAdapters_SelectedIndexChanged);

            this.lblStatus.Text = "Status:";
            this.lblStatus.Location = new System.Drawing.Point(12, 52);
            this.lblStatus.AutoSize = true;

            this.lblStatusValue.Text = "—";
            this.lblStatusValue.Location = new System.Drawing.Point(60, 52);
            this.lblStatusValue.AutoSize = true;
            this.lblStatusValue.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);

            this.grpAdapter.Controls.AddRange(new System.Windows.Forms.Control[] {
                this.cboAdapters, this.lblStatus, this.lblStatusValue
            });

            // --- Current MAC Group ---
            this.grpCurrentMac.Text = "Current MAC Addresses";
            this.grpCurrentMac.Location = new System.Drawing.Point(12, 95);
            this.grpCurrentMac.Size = new System.Drawing.Size(496, 140);

            this.lblActiveMac.Text = "Active MAC:";
            this.lblActiveMac.Location = new System.Drawing.Point(12, 28);
            this.lblActiveMac.Size = new System.Drawing.Size(100, 15);

            this.txtActiveMac.Location = new System.Drawing.Point(120, 25);
            this.txtActiveMac.Size = new System.Drawing.Size(200, 23);
            this.txtActiveMac.ReadOnly = true;
            this.txtActiveMac.BackColor = System.Drawing.SystemColors.Window;
            this.txtActiveMac.Font = new System.Drawing.Font("Consolas", 10F);

            this.lblRegistryMac.Text = "Registry MAC:";
            this.lblRegistryMac.Location = new System.Drawing.Point(12, 58);
            this.lblRegistryMac.Size = new System.Drawing.Size(100, 15);

            this.txtRegistryMac.Location = new System.Drawing.Point(120, 55);
            this.txtRegistryMac.Size = new System.Drawing.Size(200, 23);
            this.txtRegistryMac.ReadOnly = true;
            this.txtRegistryMac.BackColor = System.Drawing.SystemColors.Window;
            this.txtRegistryMac.Font = new System.Drawing.Font("Consolas", 10F);

            this.lblOriginalMac.Text = "Original MAC:";
            this.lblOriginalMac.Location = new System.Drawing.Point(12, 88);
            this.lblOriginalMac.Size = new System.Drawing.Size(100, 15);

            this.txtOriginalMac.Location = new System.Drawing.Point(120, 85);
            this.txtOriginalMac.Size = new System.Drawing.Size(200, 23);
            this.txtOriginalMac.ReadOnly = true;
            this.txtOriginalMac.BackColor = System.Drawing.SystemColors.Window;
            this.txtOriginalMac.Font = new System.Drawing.Font("Consolas", 10F);

            this.lblSpoofStatus.Text = "";
            this.lblSpoofStatus.Location = new System.Drawing.Point(340, 28);
            this.lblSpoofStatus.Size = new System.Drawing.Size(140, 40);
            this.lblSpoofStatus.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);

            this.grpCurrentMac.Controls.AddRange(new System.Windows.Forms.Control[] {
                this.lblActiveMac, this.txtActiveMac,
                this.lblRegistryMac, this.txtRegistryMac,
                this.lblOriginalMac, this.txtOriginalMac,
                this.lblSpoofStatus
            });

            // --- Change MAC Group ---
            this.grpChangeMac.Text = "Change MAC Address";
            this.grpChangeMac.Location = new System.Drawing.Point(12, 245);
            this.grpChangeMac.Size = new System.Drawing.Size(496, 185);

            this.lblNewMac.Text = "New MAC:";
            this.lblNewMac.Location = new System.Drawing.Point(12, 30);
            this.lblNewMac.AutoSize = true;

            this.txtNewMac.Location = new System.Drawing.Point(120, 27);
            this.txtNewMac.Size = new System.Drawing.Size(200, 23);
            this.txtNewMac.Font = new System.Drawing.Font("Consolas", 10F);
            this.txtNewMac.MaxLength = 17; // XX-XX-XX-XX-XX-XX
            this.txtNewMac.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            this.txtNewMac.TextChanged += new System.EventHandler(this.txtNewMac_TextChanged);

            this.lblValidation.Text = "";
            this.lblValidation.Location = new System.Drawing.Point(330, 30);
            this.lblValidation.AutoSize = true;
            this.lblValidation.Font = new System.Drawing.Font("Segoe UI", 8F);

            this.btnGenerate.Text = "🎲 Generate Random";
            this.btnGenerate.Location = new System.Drawing.Point(12, 60);
            this.btnGenerate.Size = new System.Drawing.Size(150, 32);
            this.btnGenerate.Click += new System.EventHandler(this.btnGenerate_Click);

            this.btnApply.Text = "✅ Apply MAC";
            this.btnApply.Location = new System.Drawing.Point(12, 100);
            this.btnApply.Size = new System.Drawing.Size(150, 35);
            this.btnApply.Enabled = false;
            this.btnApply.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnApply.Click += new System.EventHandler(this.btnApply_Click);

            this.btnClear.Text = "🧹 Clear Spoof";
            this.btnClear.Location = new System.Drawing.Point(170, 100);
            this.btnClear.Size = new System.Drawing.Size(150, 35);
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);

            this.btnRestore.Text = "⏪ Restore Original";
            this.btnRestore.Location = new System.Drawing.Point(328, 100);
            this.btnRestore.Size = new System.Drawing.Size(150, 35);
            this.btnRestore.Click += new System.EventHandler(this.btnRestore_Click);

            this.btnRefresh.Text = "🔄 Refresh";
            this.btnRefresh.Location = new System.Drawing.Point(170, 60);
            this.btnRefresh.Size = new System.Drawing.Size(100, 32);
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);

            this.grpChangeMac.Controls.AddRange(new System.Windows.Forms.Control[] {
                this.lblNewMac, this.txtNewMac, this.lblValidation,
                this.btnGenerate, this.btnApply, this.btnClear,
                this.btnRestore, this.btnRefresh
            });

            // --- Status Strip ---
            this.statusLabel.Text = "Ready";
            this.statusLabel.Spring = true;
            this.statusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            this.progressBar.Size = new System.Drawing.Size(100, 16);
            this.progressBar.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            this.progressBar.Visible = false;

            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.statusLabel, this.progressBar
            });

            // --- Add to form ---
            this.Controls.AddRange(new System.Windows.Forms.Control[] {
                this.grpAdapter, this.grpCurrentMac, this.grpChangeMac, this.statusStrip
            });

            this.grpAdapter.ResumeLayout(false);
            this.grpAdapter.PerformLayout();
            this.grpCurrentMac.ResumeLayout(false);
            this.grpCurrentMac.PerformLayout();
            this.grpChangeMac.ResumeLayout(false);
            this.grpChangeMac.PerformLayout();
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.GroupBox grpAdapter;
        private System.Windows.Forms.ComboBox cboAdapters;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Label lblStatusValue;

        private System.Windows.Forms.GroupBox grpCurrentMac;
        private System.Windows.Forms.Label lblActiveMac;
        private System.Windows.Forms.TextBox txtActiveMac;
        private System.Windows.Forms.Label lblRegistryMac;
        private System.Windows.Forms.TextBox txtRegistryMac;
        private System.Windows.Forms.Label lblOriginalMac;
        private System.Windows.Forms.TextBox txtOriginalMac;
        private System.Windows.Forms.Label lblSpoofStatus;

        private System.Windows.Forms.GroupBox grpChangeMac;
        private System.Windows.Forms.Label lblNewMac;
        private System.Windows.Forms.TextBox txtNewMac;
        private System.Windows.Forms.Label lblValidation;
        private System.Windows.Forms.Button btnGenerate;
        private System.Windows.Forms.Button btnApply;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.Button btnRestore;
        private System.Windows.Forms.Button btnRefresh;

        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel statusLabel;
        private System.Windows.Forms.ToolStripProgressBar progressBar;
    }
}