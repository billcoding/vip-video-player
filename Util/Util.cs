using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using Newtonsoft.Json;

namespace VVPlayer.Util
{
    public static class Downloader
    {
        public static Process CreateProcess(string execPath, string dlPath, string name, string url)
        {
            return new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = execPath,
                    Arguments = $"d -V -d \"{dlPath}\" -o \"{name}\" -t \"ts{name}\" \"{url}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    StandardOutputEncoding = Encoding.UTF8
                }
            };
        }
    }

    public class Config
    {
        public DownloadCfg DownloadCfg { get; set; }
        public static readonly string CfgPath = Environment.CurrentDirectory + "/config.json";

        public static Config GetConfig()
        {
            try
            {
                if (!File.Exists(CfgPath))
                {
                    MessageBox.Show("config.json不存在");
                    return null;
                }

                var cfgJson = File.ReadAllText(CfgPath);
                var cfg = JsonConvert.DeserializeObject<Config>(cfgJson);
                if (cfg?.DownloadCfg != null) return cfg;
                MessageBox.Show("配置格式错误");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            return null;
        }

        public static void SaveConfig(Config c)
        {
            try
            {
                var cfgJson = JsonConvert.SerializeObject(c);
                File.WriteAllText(CfgPath, cfgJson);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }

    public class DownloadCfg
    {
        public string Dir { get; set; }
        public string DownloaderExecPath { get; set; }
        public string DownloaderVersion { get; set; }
    }
}