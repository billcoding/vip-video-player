using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace VVPlayer
{
    public partial class ChooseWindow
    {
        private readonly string _name;
        private readonly string _url;

        public ChooseWindow(string name, string url)
        {
            InitializeComponent();
            _name = name;
            _url = url;

            KeyUp += delegate(object sender, KeyEventArgs e)
            {
                if (e.Key == Key.Escape)
                {
                    Close();
                }
            };

            MouseDown += delegate(object sender, MouseButtonEventArgs e)
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    Close();
                }
            };
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            var btn = (Button)sender;
            if (!(Owner is MainWindow mainWin))
            {
                return;
            }

            mainWin.CurrentVUrl = _url;
            switch (btn.Tag)
            {
                case "Play":
                    mainWin.PlayNow();
                    break;
                case "PlayList":
                    mainWin.PlayList(_name, _url);
                    break;
                case "Download":
                    mainWin.Download(_name, _url);
                    break;
            }

            Close();
        }
    }
}