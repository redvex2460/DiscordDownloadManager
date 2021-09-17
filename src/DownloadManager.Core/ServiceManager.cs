using DownloadManager.Bot.Data;
using DownloadManager.Bot.DiscordBot;
using DownloadManager.Core.Logging;
using DownloadManager.Downloader.JDownloader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DownloadManager.Bot.Core
{
    public class ServiceManager
    {

        public static DiscordBot.DiscordBot DiscordBot { get; set; }
        public static Api JDownloaderApi { get; set; }
        public static dynamic Settings => SettingsFile.Read();
        public ServiceManager()
        {
            SettingsFile.CreateFromTemplate();
            Logger.LogMessage("Creating DiscordBot...");
            DiscordBot = new DiscordBot.DiscordBot(ServiceManager.Settings.Discord.Token.ToString());
            JDownloaderApi = new Api(Settings.JDownloader.Email.ToString(), Settings.JDownloader.Password.ToString());
            while(true)
            {
                Thread.Sleep(1000);
            }
        }   
    }
}
