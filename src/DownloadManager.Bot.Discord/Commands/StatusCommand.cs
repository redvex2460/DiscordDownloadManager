using Discord.WebSocket;
using DownloadManager.Downloader.JDownloader;
using DownloadManager.Downloader.JDownloader.Models;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DownloadManager.Bot.DiscordBot
{
    class StatusCommand
    {
        internal static async Task<bool> HandleCommand(SocketSlashCommand command)
        {
            if (!Utils.UserHasDownloadManagerRole(command.User))
            {
                await command.RespondAsync("You have insufficient rights, to use this command");
                return false;
            }
            var result = await Api.Instance.QueryLinks();
            if (result != null)
            {
                StringBuilder sb = new StringBuilder();
                if (result.Count <= 0)
                {
                    await command.RespondAsync("No Downloads in queue");
                    return true;
                }
                if (command.Data.Options != null && (bool)command.Data.Options.FirstOrDefault(option => option.Name == "finished").Value == false)
                    result = (ActivePackageList)result.Where(item => !item.Status.ToLower().Contains("finished")).ToList();
                foreach (var obj in result)
                {
                    sb.AppendLine($"{obj.Name} : status: {obj.Status}");
                }
                await command.RespondAsync(sb.ToString());
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