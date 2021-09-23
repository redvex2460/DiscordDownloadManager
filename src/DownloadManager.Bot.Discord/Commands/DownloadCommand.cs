using Discord;
using Discord.WebSocket;
using DownloadManager.Bot.Data;
using DownloadManager.Downloader.JDownloader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DownloadManager.Bot.DiscordBot.Commands
{
    internal class DownloadCommand
    {
        public static async Task<bool> HandleCommand(SocketSlashCommand command)
        {
            if (!Utils.UserHasDownloadManagerRole(command.User) && ((IGuildUser)command.User).Guild.OwnerId != command.User.Id)
            {
                await command.RespondAsync("You have insufficient rights, to use this command");
                return false;
            }
            var links = string.Join("\r\n", command.Data.Options.FirstOrDefault(a => a.Name.Equals("links")).Value.ToString().Split(" "));
            var name = command.Data.Options.FirstOrDefault(a => a.Name.Equals("name")) != null ? command.Data.Options.FirstOrDefault(a => a.Name.Equals("name")).Value.ToString() : "";
            var autodownload = command.Data.Options.FirstOrDefault(a => a.Name.Equals("autodownload")) != null ? (bool)command.Data.Options.FirstOrDefault(a => a.Name.Equals("autodownload")).Value : SettingsFile.CachedSettings.JDownloader.AutoDownload;
            if (await Api.Instance.AddDownloadLink(links, name, autodownload: autodownload))
            {
                await command.RespondAsync("Worked!");
                return true;
            }
            else
            {
                await command.RespondAsync("There was an error!");
                return false;
            }
        }
    }
}
