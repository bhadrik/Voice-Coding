using System;
using System.Windows.Forms;

namespace Voice_Coding
{
    class TrayIcon
    {
        private readonly NotifyIcon notifyIcon;
        private readonly MenuItem   exit_Item;
        private readonly MenuItem   settings;

        public event EventHandler SettingClicked;
        public event EventHandler ExitCommand;

        public TrayIcon()
        {
            exit_Item = new MenuItem
            {
                Index = 0,
                Text = "Exit"
            };
            exit_Item.Click += new EventHandler(OnExitCommand);

            settings = new MenuItem
            {
                Index = exit_Item.Index - 1,
                Text = "Settings"
            };
            settings.Click += new EventHandler(OnSettingClicked);
                
            notifyIcon = new NotifyIcon
            {
                Text = "Voice Coding",
                Icon = DataResource.icon_tray_b_w,
                ContextMenu = new ContextMenu(new MenuItem[] { exit_Item, settings }),
                Visible = true
            };
        }

        protected virtual void OnSettingClicked(object sender, EventArgs e)
        {
            SettingClicked(sender, e);
        }

        protected virtual void OnExitCommand(object sender, EventArgs e)
        {
            ExitCommand(sender, e);
        }

        public void Dispose()
        {
            notifyIcon.Dispose();
        }
    }
}
