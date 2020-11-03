using System;
using System.Windows;
using System.Windows.Media;

namespace Voice_Coding
{
    public partial class StatusBar : Window
    {
        public event EventHandler<RoutedEventArgs> ToggleRecogniton;
        public event EventHandler<RoutedEventArgs> Exit;
        private Color turnOn, turnOff;

        public StatusBar()
        {
            InitializeComponent();
            Left = 0;
            Top = SystemParameters.WorkArea.Height - Height;

            turnOn  = Color.FromArgb(255, 64, 192, 117);
            turnOff = Color.FromArgb(255, 121, 121, 121);
        }

        public void ToggleColor(bool on)
        {
            if (on) toggleBtn.Background = new SolidColorBrush(turnOn);
            else toggleBtn.Background = new SolidColorBrush(turnOff);
        }

        private void ToggleBtn_Click(object sender, RoutedEventArgs e)
        {
            ToggleRecogniton?.Invoke(this, e);
        }

        private void ExitStatusBar_Click(object sender, RoutedEventArgs e)
        {
            Exit?.Invoke(sender, e);
        }

        public void ChangeText(String str)
        {
            status.Content = str;
        }
    }
}
