using Discord;
using Discord.WebSocket;
using DownloadManager.Bot.Data;
using System.Linq;

namespace DownloadManager.Bot.DiscordBot
{
    public static class Utils
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

        public static IGuild GetGuildOfChannelId(this DiscordSocketClient client, ulong id)
        {
            return client.Guilds.FirstOrDefault(guild => guild.Channels.FirstOrDefault(channel => channel.Id == id) != null);
        }

        public static IRole GetRole(this IGuild guild, string name)
        {
            return guild.Roles.FirstOrDefault(role => role.Name.ToLower().Equals(name.ToLower()));
        }
    }
}