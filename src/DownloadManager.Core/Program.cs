using DownloadManager.Bot.Discord;
using DownloadManager.Downloader.JDownloader;
using DownloadManager.Core.Logging;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using DownloadManager.Bot.Core;

namespace DownloadManager.Core
{
    public class Program
    {
        public static ServiceManager ServiceManager { get; set; }
        static void Main(string[] args)
        {
            Logger.LogMessage($"Starting DownloadManager {Environment.Version} by RedVex2460");
            ServiceManager = new ServiceManager();
        }
    }
}
