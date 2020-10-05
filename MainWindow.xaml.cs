using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Forms;
using Voice_Coding.src;

namespace Voice_Coding
{
    public partial class MainWindow : Window
    {
        int level = 0;
        //Recognizer objects
        private CodeRecognition rec = new CodeRecognition();

        //Tray icon
        private NotifyIcon notifyIcon;
        private ContextMenu menu;
        private MenuItem exit_Item;

        public MainWindow()
        {
            InitializeComponent();

            menu = new System.Windows.Forms.ContextMenu();
            exit_Item = new System.Windows.Forms.MenuItem();

            menu.MenuItems.AddRange(new MenuItem[] { exit_Item });

            exit_Item.Index = 0;
            exit_Item.Text = "Exit";
            exit_Item.Click += new EventHandler(ExitApp);

            notifyIcon = new System.Windows.Forms.NotifyIcon();
            notifyIcon.Text = "Voice Coding";
            notifyIcon.Icon = CodeRecognition.getIcon();
            notifyIcon.Click += new EventHandler(NotifyIcon_Click);
            notifyIcon.ContextMenu = menu;
            notifyIcon.Visible = true;

            rec.startRecognition();
            rec.ExitEvent += new EventHandler(ExitApp);
        }

        public void NotifyIcon_Click(object source, EventArgs e)
        {
            if (rec.recognising)
            {
                rec.stopRecognition();
                rec.recognising = false;
            }
            else
            {
                rec.startRecognition();
                rec.recognising = true;
            }

            this.Show();
        }

        private void ExitApp(object sender, EventArgs e)
        {
            notifyIcon.Dispose();
            this.Close();
        }

        private void OnMainWindowClose(Object src, CancelEventArgs e)
        {
            notifyIcon.Dispose();
        }

        private void OnDrag(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //base.OnMouseLeftButtonDown(e);
            this.DragMove();
        }

        private void HideToTray(object sender, EventArgs e)
        {
            this.Hide();
        }
    }
}
