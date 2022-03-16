using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Web.WebView2.Core;
using VVPlayer.Util;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using Label = System.Windows.Controls.Label;
using MenuItem = System.Windows.Controls.MenuItem;

namespace VVPlayer
{
    /// <summary>
    ///     MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow
    {
        public string CurrentVUrl = "";
        private string _currentCUrl = "";
        public Config Cfg;

        private void TxtText_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;
            if (e.IsRepeat) return;
            var search = TextBoxText.Text.Trim();
            if (search == "") return;
            if (!(CbxType.SelectedItem is ComboBoxItem vcb))
            {
                return;
            }

            var url = vcb.Tag.ToString()
                .Replace("@TEXT@", search)
                .Replace("%26", "&");
            WebViewSearch.Source = new Uri(url);
            TabControl.SelectedIndex = 0;
        }

        private void ListViewChannelOnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!(ListViewChannel.SelectedItem is Label lbl)) return;
            if (lbl.Tag.ToString() == _currentCUrl) return;
            _currentCUrl = lbl.Tag.ToString();
            PlayNow();
        }

        private void MenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            if (!(ListViewPlayList.SelectedItem is Label lbl)) return;
            var name = lbl.Content.ToString();
            CurrentVUrl = lbl.Tag.ToString();
            var menuItem = sender as MenuItem;
            switch (menuItem?.Tag.ToString())
            {
                case "Play":
                    PlayNow();
                    break;
                case "Delete":
                    ListViewPlayList.Items.RemoveAt(ListViewPlayList.SelectedIndex);
                    break;
                case "Download":
                    Download(name, CurrentVUrl);
                    break;
                case "Clear":
                    ListViewPlayList.Items.Clear();
                    break;
            }
        }

        private void MenuItem_OnClick2(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            switch (menuItem?.Tag.ToString())
            {
                case "DownloadSetting":
                    var dlSettingWin = new DownloadSettingWindow { Owner = this };
                    dlSettingWin.ShowDialog();
                    break;
                case "Copyright":
                    var aboutWin = new AboutWindow { Owner = this };
                    aboutWin.ShowDialog();
                    break;
                case "Exit":
                    Environment.Exit(0);
                    break;
                case "ClearLog":
                    ListViewLog.Items.Clear();
                    break;
                case "KillDownloader":
                    try
                    {
                        var p = new Process
                        {
                            StartInfo = new ProcessStartInfo
                            {
                                FileName = "taskkill.exe",
                                Arguments = "/IM vip-video-downloader.exe /F",
                                UseShellExecute = false,
                                CreateNoWindow = true,
                                RedirectStandardOutput = true,
                                RedirectStandardError = true,
                            }
                        };
                        p.Start();
                    }
                    catch (Exception)
                    {
                        //
                    }

                    break;
            }
        }

        private void ListViewPlayList_OnContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (ListViewPlayList.Items.Count <= 0)
            {
                e.Handled = true;
            }
        }

        private async void NewWindowRequested(object sender, CoreWebView2NewWindowRequestedEventArgs e)
        {
            var uri = e.Uri;
            if (string.IsNullOrEmpty(uri))
            {
                return;
            }

            e.Handled = true;
            if (!IsVideoUri(uri))
            {
                return;
            }

            AppendLog(uri);
            var videoName = await GetVideoName(uri);
            var chooseWindow = new ChooseWindow(videoName, e.Uri)
            {
                Owner = this
            };
            chooseWindow.ShowDialog();
        }

        private void CoreWebView2InitializationCompleted(object sender, CoreWebView2InitializationCompletedEventArgs e)
        {
            WebViewSearch.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
            WebViewSearch.CoreWebView2.Settings.AreDevToolsEnabled = true;
            WebViewSearch.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(@"
if(location.href.indexOf('so.iqiyi.com') != -1){
    var linkNodes  = document.querySelectorAll('a.qy-mod-link');
    linkNodes.forEach(linkNode =>{
        linkNode.replaceWith(linkNode.cloneNode(true));
    });
}
");
            WebViewSearch.CoreWebView2.NewWindowRequested += NewWindowRequested;
        }


        public MainWindow()
        {
            InitializeComponent();
            LoadPlatforms();
            LoadChannels();
            ListViewChannel.SelectionChanged += ListViewChannelOnSelectionChanged;
            WebViewSearch.CoreWebView2InitializationCompleted += CoreWebView2InitializationCompleted;

            Cfg = Config.GetConfig();
            if (Cfg == null)
            {
                MessageBox.Show("加载配置失败。");
            }
        }

        /// <summary>
        /// 立即播放
        /// </summary>
        public void PlayNow()
        {
            if (_currentCUrl == string.Empty || CurrentVUrl == string.Empty) return;
            WebViewPlayer.Source = new Uri(_currentCUrl + CurrentVUrl);
            TabControl.SelectedIndex = 1;
        }

        public void PlayList(string title, string url)
        {
            var lbl = new Label { Content = title, Tag = url };
            ListViewPlayList.Items.Add(lbl);
        }

        public void Download(string name, string url)
        {
            var execPath = "";
            var dlPath = "";
            if (Cfg?.DownloadCfg != null && !string.IsNullOrEmpty(Cfg.DownloadCfg.DownloaderExecPath) &&
                !string.IsNullOrEmpty(Cfg.DownloadCfg.Dir))
            {
                execPath = Cfg.DownloadCfg.DownloaderExecPath;
                dlPath = Cfg.DownloadCfg.Dir;
            }

            if (string.IsNullOrEmpty(execPath))
            {
                MessageBox.Show("未安装下载器");
                return;
            }

            if (string.IsNullOrEmpty(dlPath))
            {
                MessageBox.Show("读取下载文件夹配置错误");
                return;
            }

            TabControl.SelectedIndex = 2;
            var p = Downloader.CreateProcess(execPath, dlPath, name, url);
            new Thread(() =>
            {
                p.OutputDataReceived += delegate(object sender, DataReceivedEventArgs e)
                {
                    var m = e.Data;
                    if (!string.IsNullOrEmpty(m))
                    {
                        Dispatcher.InvokeAsync(() => AppendLog(m));
                    }
                };
                p.Start();
                p.BeginOutputReadLine();
                p.BeginErrorReadLine();
            }).Start();
        }

        private void LoadPlatforms()
        {
            CbxType.Items.Add(new ComboBoxItem
                { Content = "优酷视频", Tag = "https://so.youku.com/search_video/q_@TEXT@?searchfrom=1" });
            CbxType.Items.Add(new ComboBoxItem
                { Content = "爱奇艺", Tag = "https://so.iqiyi.com/so/q_@TEXT@?source=input" });
            CbxType.Items.Add(new ComboBoxItem
                { Content = "腾讯视频", Tag = "https://v.qq.com/x/search/?q=@TEXT@%26stag=0%26smartbox_ab=" });
            CbxType.Items.Add(new ComboBoxItem
            {
                Content = "搜狐TV",
                Tag = "https://so.tv.sohu.com/mts?wd=@TEXT@%26time=1642996522871%26code=2IjN4YTN5gTO4ITO0QmZnZGZnNHZ"
            });
            CbxType.Items.Add(new ComboBoxItem
                { Content = "芒果TV", Tag = "https://so.mgtv.com/so?k=@TEXT@%26lastp=ch_home" });
            CbxType.Items.Add(new ComboBoxItem
            {
                Content = "乐视视频",
                Tag =
                    "http://so.le.com/s?wd=@TEXT@%26from=pc%26index=0%26ref=click%26click_area=search_button%26query=vhgfh%26is_default_query=0%26module=search_rst_page"
            });
        }

        private void LoadChannels()
        {
            var chs = new List<string>
            {
                "https://jx.parwix.com:4433/player/?url=",
                "http://42.193.18.62:9999/?url=",
                "https://www.1717yun.com/jx/ty.php?url=",
                "https://okjx.cc/?url=",
                "https://www.ckmov.vip/api.php?url=",
                "https://z1.m1907.cn/?jx=",
                "https://ckmov.ccyjjd.com/ckmov/?url=",
                "https://v.znb.me/?url=",
                "https://123.1dior.cn/?url=",
                "https://www.h8jx.com/jiexi.php?url=",
                "https://www.ckplayer.vip/jiexi/?url="
            };
            var i = 0;
            foreach (var lbl in from c in chs
                     let cc = c.Trim()
                     where cc != string.Empty
                     select new Label
                     {
                         Content = $"通道{++i}",
                         Tag = cc
                     })
            {
                ListViewChannel.Items.Add(lbl);
            }

            if (ListViewChannel.Items.Count > 0)
            {
                _currentCUrl = (ListViewChannel.Items[0] as Label)?.Tag.ToString();
            }
        }

        private void AppendLog(string log)
        {
            var logStr = $"[{DateTime.Now:s}] {log}";
            ListViewLog.Items.Add(new Label { Content = logStr });
            ScrollViewer.ScrollToEnd();
        }

        private async Task<string> GetVideoName(string uri)
        {
            var videoName = "";
            var vUri = uri.Replace("https:", "").Replace("http:", "");
            var getJs = "";
            switch (CbxType.SelectedIndex)
            {
                case 0: // 优酷

                    //兼容：https://v.youku.com/v_show/id_XNTg0NzEyODIyNA==.html

                    //兼容：https://v.youku.com/v_nextstage/id_fdcd70c72b0740d68709.html
                    getJs =
                        $"document.querySelector('a[href=\"{vUri}\"]').parentElement.title.replaceAll(' ','')"
                        + "||"
                        + $"JSON.parse(document.querySelector('a[href=\"{vUri}\"]').getAttribute(\"data-trackinfo\")).object_title";
                    videoName = await WebViewSearch.CoreWebView2.ExecuteScriptAsync(getJs);
                    videoName = videoName?.Trim('\"');
                    AppendLog($"GetVideoName getJs[{getJs}] videoName[{videoName}]");

                    break;

                case 1: // 爱奇艺

                    //兼容：https://www.iqiyi.com/v_2g3kscvft04.html
                    getJs = $"document.querySelector('a[href=\"{vUri}\"]').title";
                    videoName = await WebViewSearch.CoreWebView2.ExecuteScriptAsync(getJs);
                    videoName = videoName?.Trim('\"');
                    AppendLog($"GetVideoName getJs[{getJs}] videoName[{videoName}]");

                    break;

                case 2: // 腾讯视频

                    //兼容：https://v.qq.com/x/cover/mzc00200rh9pv6b.html
                    getJs = $"document.querySelector('a[href=\"{vUri}\").alt.replace(/[\x05\x06]/g,'')";
                    videoName = await WebViewSearch.CoreWebView2.ExecuteScriptAsync(getJs);
                    videoName = videoName?.Trim('\"');
                    AppendLog($"GetVideoName getJs[{getJs}] videoName[{videoName}]");

                    break;

                case 3: // 搜狐TV

                    break;

                case 4: // 芒果TV

                    break;

                case 5: // 乐视视频

                    break;
            }

            return videoName;
        }


        private bool IsVideoUri(string uri)
        {
            return uri.Contains("v.youku.com/v_show/id_") || uri.Contains("v.youku.com/v_nextstage/id") // 优酷视频
                                                          || uri.Contains("www.iqiyi.com/v_") // 爱奇艺
                                                          || uri.Contains("v.qq.com/x/cover/") // 腾讯视频
                ;
        }
    }
}