using Discord;
using Discord.WebSocket;
using DownloadManager.Bot.Data;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DownloadManager.Bot.DiscordBot
{
    class PermitCommand
    {
        internal static async Task<bool> HandleCommand(SocketSlashCommand command)
        {
            if (!Utils.UserHasDownloadManagerRole(command.User) && ((IGuildUser)command.User).Guild.OwnerId != command.User.Id)
            {
                await command.RespondAsync("You have insufficient rights, to use this command");
                return false;
            }
            var user = command.Data.Options.FirstOrDefault(option => option.Name == "user").Value as IGuildUser;
            var role = user.Guild.GetRole(SettingsFile.CachedSettings.Discord.Role);
            await user.AddRoleAsync(role);
            await command.RespondAsync($"{user.Username} added the {SettingsFile.CachedSettings.Discord.Role} role");
            return true;
        }
    }
}