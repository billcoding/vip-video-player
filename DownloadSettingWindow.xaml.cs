using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Forms;
using Button = System.Windows.Controls.Button;
using MessageBox = System.Windows.MessageBox;

namespace VVPlayer
{
    public partial class DownloadSettingWindow : Window
    {
        public DownloadSettingWindow()
        {
            InitializeComponent();
        }

        private void Button_OnClick(object sender, RoutedEventArgs e)
        {
            var btn = (Button)sender;
            switch (btn.Name)
            {
                case "ButtonBrowse":
                    BrowseFolder();
                    break;
                case "ButtonInstall":
                    InstallDownloader();
                    break;
                case "ButtonSetup":
                    SetupDownloader();
                    break;
            }
        }

        private void BrowseFolder()
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

        private void SetupDownloader()
        {
            var fbd = new OpenFileDialog
                { Filter = @"exe文件|*.exe", CheckFileExists = true, Multiselect = false, AddExtension = false };
            var sd = fbd.ShowDialog();
            if (sd == System.Windows.Forms.DialogResult.OK)
            {
                var selectFile = fbd.FileName;
                // vip-video-downloader version 1.0.4

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
                TextBoxDownloaderExecPath.Text = selectFile;
            }
        }

        private static string VerifyDownloader(string fileName)
        {
            try
            {
                var p = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = fileName,
                        Arguments = "-v",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    }
                };
                p.OutputDataReceived += delegate(object sender, DataReceivedEventArgs e)
                {
                    
                };
                p.Start();
                p.BeginOutputReadLine();
            }
            catch (Exception)
            {
                // ignored
            }

            return null;
        }
    }
}