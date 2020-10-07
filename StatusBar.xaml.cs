using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms.VisualStyles;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Voice_Coding
{
    public partial class StatusBar : Window
    {
        public event EventHandler<RoutedEventArgs> toggleRecogniton;
        private System.Windows.Media.Color turnOn, turnOff;

        public StatusBar()
        {
            InitializeComponent();
            Left = 0;
            Top = SystemParameters.WorkArea.Height - Height;

            turnOn = System.Windows.Media.Color.FromArgb(255, 34, 208, 142);
            turnOff = System.Windows.Media.Color.FromArgb(255, 208, 34, 34);
            toggleBtn.Background = new SolidColorBrush(turnOn);
        }

        private void toggleBtn_Click(object sender, RoutedEventArgs e)
        {
            if (toggleRecogniton != null)
                toggleRecogniton(this, e);
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
