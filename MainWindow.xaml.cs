#define Notify

using System;
using System.ComponentModel;
using System.Windows;
using Voice_Coding.src;

namespace Voice_Coding
{
    public partial class MainWindow : Window
    {
        //int level = 0;

        private readonly CodeRecognition rec;
        private enum ClassType
        {
            MenuItem,
            StatusBar,
        }

#if Notify
        //Tray icon
        private readonly TrayIcon notifyIcon;
#endif

        public MainWindow()
        {
            InitializeComponent();
            //Set window to the bottom right corner
            Left = SystemParameters.WorkArea.Width - Width - 10;
            Top = SystemParameters.WorkArea.Height - Height - 10;

            rec = new CodeRecognition();
            rec.StartRecognition(false);
            rec.ExitEvent += new EventHandler(ExitApp);

#if Notify
            notifyIcon = new TrayIcon();
            notifyIcon.SettingClicked += new EventHandler(OpenSettingsMenu);
            notifyIcon.ExitCommand += new EventHandler(ExitApp);
#endif
            this.Closing += new CancelEventHandler(ClosingWindow);
        }

        private void ClosingWindow(object sender, CancelEventArgs e)
        {
#if Notify
            notifyIcon.Dispose();
#endif
            rec.Close();
        }

        private void ExitApp(object sender, EventArgs e)
        {
            this.Close();
        }

        private void OpenSettingsMenu(object sender, EventArgs e)
        {
            this.Show();
        }

        private void HideToTray(object sender, EventArgs e)
        {
            this.Hide();
        }
    }
}
