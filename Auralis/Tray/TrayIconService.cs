using System;
using System.Drawing;
using System.Windows;
using System.Windows.Forms;

namespace Auralis.Tray
{
    public sealed class TrayIconService : IDisposable
    {
        private readonly NotifyIcon _notifyIcon;

        public event Action? EnableStudyModeRequested;
        public event Action? DisableStudyModeRequested;
        public event Action? ExitRequested;
        public event Action? SettingsRequested;


        private bool _studyModeEnabled;

        public TrayIconService()
        {
            _notifyIcon = new NotifyIcon
            {
                // REVIEW LATER: Replace with custom icon
                Icon = SystemIcons.Exclamation,
                Text = "Auralis",
                Visible = true,
                ContextMenuStrip = BuildMenu()
            };
        }

        private ContextMenuStrip BuildMenu()
        {
            var menu = new ContextMenuStrip();

            var toggleItem = new ToolStripMenuItem("Enable");
            toggleItem.Click += (_, _) => ToggleStudyMode(toggleItem);

            var settingsItem = new ToolStripMenuItem("Settings");
            settingsItem.Click += (_, _) => SettingsRequested?.Invoke();
            

            var exitItem = new ToolStripMenuItem("Exit");
            exitItem.Click += (_, _) => ExitRequested?.Invoke();

            menu.Items.Add(toggleItem);
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(settingsItem);
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(exitItem);

            return menu;
        }

        public void UpdateStatus(string status)
        {
            _notifyIcon.Text = $"Auralis - {status}";
        }  

        private void ToggleStudyMode(ToolStripMenuItem item)
        {
            _studyModeEnabled = !_studyModeEnabled;

            item.Text = _studyModeEnabled
                ? "Disable"
                : "Enable";

            if (_studyModeEnabled)
                EnableStudyModeRequested?.Invoke();
            else
                DisableStudyModeRequested?.Invoke();
        }

        public void Dispose()
        {
            _notifyIcon.Visible = false;
            _notifyIcon.Dispose();
        }
    }
}
