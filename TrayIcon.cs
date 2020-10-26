using System;
using System.Windows.Forms;

namespace Voice_Coding
{
    class TrayIcon
    {
        private readonly NotifyIcon notifyIcon;
        private readonly MenuItem exit_Item;
        private readonly MenuItem settings;

        public event EventHandler SettingClicked;
        public event EventHandler ExitCommand;

        public TrayIcon()
        {
            exit_Item = new MenuItem();
            settings = new MenuItem();

            settings.Index = 0;
            settings.Text = "Settings";
            settings.Click += new EventHandler(OpenSettingsMenu);

            exit_Item.Index = settings.Index + 1;
            exit_Item.Text = "Exit";
            exit_Item.Click += new EventHandler(ExitApp);

            notifyIcon = new NotifyIcon
            {
                Text = "Voice Coding",
                Icon = DataResource.icon_tray_b_w,
                ContextMenu = new ContextMenu(new MenuItem[] { exit_Item, settings }),
                Visible = true
            };
        }

        private void ExitApp(object sender, EventArgs e)
        {
            OnExitCommand(e);
        }

        private void OpenSettingsMenu(object sender, EventArgs e)
        {
            OnSettingClicked(e);
        }

        protected virtual void OnSettingClicked(EventArgs e)
        {
            SettingClicked(this, e);
        }

        protected virtual void OnExitCommand(EventArgs e)
        {
            ExitCommand(this, e);
        }

        public void Dispose()
        {
            notifyIcon.Dispose();
        }
    }
}
