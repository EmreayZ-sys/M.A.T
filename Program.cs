using System;
using System.Security.Principal;
using System.Windows.Forms;
using MACAddressTool.UI;

namespace MACAddressTool
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            if (!IsRunningAsAdmin())
            {
                MessageBox.Show(
                    "This application requires administrator privileges.\n" +
                    "Please right-click and select 'Run as administrator'.",
                    "Administrator Required",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }

        private static bool IsRunningAsAdmin()
        {
            try
            {
                var identity = WindowsIdentity.GetCurrent();
                var principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch
            {
                return false;
            }
        }
    }
}