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

        public MainWindow()
        {
            InitializeComponent();

            rec = new CodeRecognition();

            menu = new System.Windows.Forms.ContextMenu();
            exit_Item = new System.Windows.Forms.MenuItem();

            menu.MenuItems.AddRange(new MenuItem[] { exit_Item });

            exit_Item.Index = 0;
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
        }

        private void ExitApp(object sender, EventArgs e)
        {
            notifyIcon.Dispose();
            rec.Close();
            this.Close();
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
