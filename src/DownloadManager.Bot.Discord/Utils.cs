using Discord;
using Discord.WebSocket;
using DownloadManager.Bot.Data;
using System.Linq;

namespace DownloadManager.Bot.DiscordBot
{
    internal class Utils
    {
        #region Public Methods

        /// <summary>
        /// Checks if the <paramref name="user"/> is a bot
        /// </summary>
        /// <param name="user">SocketUser</param>
        /// <returns>true or false</returns>
        public static bool IsUserABot(IUser user) => user.IsBot;

        /// <summary>
        /// Checks if the <paramref name="user"/> has designated role
        /// </summary>
        /// <param name="user"></param>
        /// <returns>true or false</returns>
        public static bool UserHasDownloadManagerRole(IUser user)
        {
            SocketGuildUser guildUser = user as SocketGuildUser;
            if (guildUser.Roles.FirstOrDefault(role => role.Name.ToLower() == SettingsFile.CachedSettings.Discord.Role.ToLower()) != null)
                return true;
            return false;
        }

        #endregion Public Methods
    }
}