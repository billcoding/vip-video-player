using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Threading;

namespace VVPlayer.Util
{
    public static class Downloader
    {
        public static Process CreateProcess(string dlPath, string name, string url)
        {
            return new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "vip-video-downloader.exe",
                    Arguments = $"d -V -d \"{dlPath}\" -o {name} -t ts{name} {url}",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    StandardOutputEncoding = Encoding.UTF8
                }
            };
        }
    }
}