#define Notify

using System;
using System.ComponentModel;
using System.Windows;
using Voice_Coding.src;
using System.Windows.Input;
using System.Xml;

namespace Voice_Coding
{
    public partial class MainWindow : Window
    {
        private readonly CodeRecognition rec;

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

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(DataResource.MainResource);

            XmlNodeList nodes = doc.GetElementsByTagName("HeaderFiles");

            foreach(XmlNode node in nodes)
            {
                if(node.Attributes["type"].Value == "custom")
                {
                    IncluedPath.Text = node.Attributes["value"].Value;
                    break;
                }
            }

            IncluedPath.KeyUp += new KeyEventHandler(OnKeyUp);

            ReloadBtn.Click += new RoutedEventHandler(OnReloadClick);
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

        private void OnKeyUp(object sender, KeyEventArgs key)
        {
            if (key.Key == Key.Enter)
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(DataResource.MainResource);

                XmlNodeList nodes = doc.GetElementsByTagName("HeaderFiles");

                foreach(XmlNode node in nodes)
                {
                    if (node.Attributes["type"].Value.Equals("custom"))
                    {
                        node.Attributes["value"].Value = IncluedPath.Text;
                        Console.WriteLine("Changed text:" + IncluedPath.Text);
                        break;
                    }
                }
                doc.Save(@"..\..\res\MainResource.xml");
                rec.ReloadGrammar();
            }
        }

        private void OnReloadClick(object sender, EventArgs e)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(@"..\..\res\MainResource.xml");

            XmlNodeList nodes = doc.GetElementsByTagName("HeaderFiles");

            foreach (XmlNode node in nodes)
            {
                if (node.Attributes["type"].Value.Equals("custom"))
                {
                    node.Attributes["value"].Value = IncluedPath.Text;
                    Console.WriteLine("Changed text:" + IncluedPath.Text);
                    break;
                }
            }
            doc.Save(@"..\..\res\MainResource.xml");
            rec.ReloadGrammar();
        }
    }
}
