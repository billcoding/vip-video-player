using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using VVPlayer.Util;
using Button = System.Windows.Controls.Button;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MessageBox = System.Windows.MessageBox;

namespace VVPlayer
{
    public partial class DownloadSettingWindow : Window
    {
        public DownloadSettingWindow()
        {
            InitializeComponent();

            var cfg = Config.GetConfig();
            if (cfg.DownloadCfg != null)
            {
                TextBoxDownloadPath.Text = cfg.DownloadCfg.Dir ?? "";
                TextBoxDownloaderExecPath.Text = cfg.DownloadCfg.DownloaderExecPath ?? "";
                LabelDownloaderVersion.Content = cfg.DownloadCfg.DownloaderVersion ?? "";
            }

            KeyUp += delegate(object sender, KeyEventArgs e)
            {
                if (e.Key == Key.Escape)
                {
                    Close();
                }
            };
        }

        private void Button_OnClick(object sender, RoutedEventArgs e)
        {
            var btn = (Button)sender;
            switch (btn.Tag)
            {
                case "DirBrowse":
                    DirBrowse();
                    break;
                case "Install":
                    InstallDownloader();
                    break;
                case "ExecBrowse":
                    ExecBrowse();
                    break;
                case "Save":
                    SaveConfig();
                    var mainWindow = (MainWindow)Owner;
                    if (mainWindow != null) mainWindow.Cfg = Config.GetConfig();
                    break;
            }
        }

        private void DirBrowse()
        {
            var fbd = new FolderBrowserDialog();
            fbd.ShowNewFolderButton = true;
            fbd.Description = @"下载文件保存路径";
            var sd = fbd.ShowDialog();
            if (sd == System.Windows.Forms.DialogResult.OK)
            {
                var selectPath = fbd.SelectedPath;
                if (!string.IsNullOrEmpty(selectPath))
                {
                    TextBoxDownloadPath.Text = selectPath;
                }
            }
        }

        private static void InstallDownloader()
        {
            try
            {
                Process.Start("http://github.com/billcoding/vip-video-downloader/releases");
            }
            catch (Exception)
            {
                // Ignore
            }
        }

        private void ExecBrowse()
        {
            var fbd = new OpenFileDialog
                { Filter = @"exe文件|*.exe", CheckFileExists = true, Multiselect = false, AddExtension = false };
            var sd = fbd.ShowDialog();
            if (sd != System.Windows.Forms.DialogResult.OK) return;
            var selectFile = fbd.FileName;

            var versionInfo = VerifyDownloader(selectFile);
            var valid = !string.IsNullOrEmpty(versionInfo);
            var dlVersion = "";

            if (valid)
            {
                var vis = versionInfo.Split(' ');
                if (vis.Length == 3)
                {
                    var v1 = vis[0].Trim();
                    var v2 = vis[1].Trim();
                    dlVersion = vis[2].Trim();
                    valid = v1 == "vip-video-downloader" && v2 == "version";
                }
            }

            if (!valid)
            {
                MessageBox.Show("不合法的下载器。");
                return;
            }

            MessageBox.Show("合法的下载器。版本：" + dlVersion);
            LabelDownloaderVersion.Content = dlVersion;
            TextBoxDownloaderExecPath.Text = selectFile;
        }

        private static string VerifyDownloader(string fileName)
        {
            var vInfo = "";
            try
            {
                var p = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = fileName,
                        Arguments = "-v",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    }
                };
                p.Start();
                vInfo = p.StandardOutput.ReadLine();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            return vInfo;
        }

        private void SaveConfig()
        {
            var cfg = new Config
            {
                DownloadCfg = new DownloadCfg
                {
                    Dir = TextBoxDownloadPath.Text.Trim(),
                    DownloaderExecPath = TextBoxDownloaderExecPath.Text.Trim(),
                    DownloaderVersion = LabelDownloaderVersion.Content.ToString().Trim()
                }
            };
            Config.SaveConfig(cfg);
        }
    }
}