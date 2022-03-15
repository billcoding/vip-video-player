using System.Diagnostics;
using System.Windows;

namespace VVPlayer
{
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            Process.Start("https://github.com/billcoding/vip-video-player");
        }
    }
}