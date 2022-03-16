using System;
using System.IO;
using System.Windows;
using Newtonsoft.Json;
using VVPlayer.Util;

namespace VVPlayer
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        private App()
        {
            CheckConfig();
        }

        private static void CheckConfig()
        {
            var cfgPath = Config.CfgPath;
            if (File.Exists(cfgPath)) return;
            var cfgJson = JsonConvert.SerializeObject(new Config { DownloadCfg = new DownloadCfg() });
            try
            {
                File.WriteAllText(cfgPath, cfgJson);
            }
            catch (Exception)
            {
                // Ignore
            }
        }
    }
}