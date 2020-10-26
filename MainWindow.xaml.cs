using System;
using System.ComponentModel;
using System.Windows;
using Voice_Coding.src;

namespace Voice_Coding
{
    public partial class MainWindow : Window
    {
        int level = 0;

        //Recognizer objects
        private readonly CodeRecognition rec;

        //Tray icon
        private readonly TrayIcon notifyIcon;

        public MainWindow()
        {
            InitializeComponent();
            //Set window to the bottom right corner
            Left = SystemParameters.WorkArea.Width - Width - 10;
            Top = SystemParameters.WorkArea.Height - Height - 10;

            rec = new CodeRecognition();
            rec.StartRecognition(false);
            rec.ExitEvent += new EventHandler(ExitApp);

            notifyIcon = new TrayIcon();
            notifyIcon.SettingClicked += new EventHandler(OpenSettingsMenu);
            notifyIcon.ExitCommand += new EventHandler(ExitApp);

            this.Closing += new CancelEventHandler(ClosingWindow);
        }

        private void ClosingWindow(object sender, CancelEventArgs e)
        {
            notifyIcon.Dispose();
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
