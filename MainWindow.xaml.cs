using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Forms;
using Voice_Coding.src;

namespace Voice_Coding
{
    public partial class MainWindow : Window
    {
        readonly int level = 0;
        //Recognizer objects
        private readonly CodeRecognition rec;

        //Tray icon
        private readonly NotifyIcon notifyIcon;
        private readonly ContextMenu menu;
        private readonly MenuItem exit_Item;
        private readonly MenuItem settings;

        public MainWindow()
        {
            InitializeComponent();

            rec = new CodeRecognition();

            menu = new ContextMenu();
            exit_Item = new MenuItem();
            settings = new MenuItem();

            menu.MenuItems.AddRange(new MenuItem[] { exit_Item, settings });

            settings.Index = 0;
            settings.Text = "Settings";
            settings.Click += new EventHandler(OpenSettingsMenu);

            exit_Item.Index = settings.Index + 1;
            exit_Item.Text = "Exit";
            exit_Item.Click += new EventHandler(ExitApp);

            notifyIcon = new System.Windows.Forms.NotifyIcon();
            notifyIcon.Text = "Voice Coding";
            notifyIcon.Icon = CodeRecognition.getIcon();
            notifyIcon.ContextMenu = menu;
            notifyIcon.Visible = true;

            rec.startRecognition();
            rec.ExitEvent += new EventHandler(ExitApp);
            SetWindowToBottomRightOfScreen();

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

        private void SetWindowToBottomRightOfScreen()
        {
            Left = SystemParameters.WorkArea.Width - Width - 10;
            Top = SystemParameters.WorkArea.Height - Height - 10;
        }

        private void HideToTray(object sender, EventArgs e)
        {
            this.Hide();
        }
    }
}
