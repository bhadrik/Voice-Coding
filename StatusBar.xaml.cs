using System;
using System.Windows;
using System.Windows.Media;

namespace Voice_Coding
{
    public partial class StatusBar : Window
    {
        public event EventHandler<RoutedEventArgs> toggleRecogniton;
        private Color turnOn, turnOff;

        public StatusBar()
        {
            InitializeComponent();
            Left = 0;
            Top = SystemParameters.WorkArea.Height - Height;

            turnOn = Color.FromArgb(255, 34, 208, 142);
            turnOff = Color.FromArgb(255, 208, 34, 34);
            toggleBtn.Background = new SolidColorBrush(turnOn);
        }

        private void toggleBtn_Click(object sender, RoutedEventArgs e)
        {
            toggleRecogniton?.Invoke(this, e);
        }

        public void toggleColor(bool on)
        {
            if(on) toggleBtn.Background = new SolidColorBrush(turnOn);
            else toggleBtn.Background = new SolidColorBrush(turnOff);
        }

        public void changeText(String str)
        {
            status.Content = str;
        }
    }
}
