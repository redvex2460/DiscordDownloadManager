using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DownloadManager.Bot.Discord
{
    class Utils
    {
        public static bool IsUserABot(SocketUser user)
        {
            return user.IsBot;
        }

        public static bool UserHasDownloadManagerRole(SocketUser user)
        {
            SocketGuildUser guildUser = user as SocketGuildUser;
            if (guildUser.Roles.FirstOrDefault(role => role.Name.ToLower() == "DownloadManager".ToLower()) != null)
                return true;
            return false;
        }
    }
}
