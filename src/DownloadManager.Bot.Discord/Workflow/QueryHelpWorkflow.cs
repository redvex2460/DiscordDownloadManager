using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace DownloadManager.Bot.Discord.Workflow
{
    internal class QueryHelpWorkflow : WorkflowModule
    {
        SocketUser _user { get { return _context.User; } }
        ISocketMessageChannel _channel { get { return _context.Channel; } }
        public QueryHelpWorkflow(SocketCommandContext context, DiscordSocketClient discordBot) : base(context, discordBot)
        {
        }

        internal async Task HandleQueryPackagesRequestAsync()
        {
            if (_user.IsBot) return;

            if (!Utils.UserHasDownloadManagerRole(_user))
            {
                await ReplyToUserAsync("Sorry you have insufficient rights to use this bot, please contact the host!");
                return;
            }

            await ReplyToUserAsync("I´m glad you asked!\r\n" +
                "These are the current available commands:\r\n" +
                "!download <link/s> -> you add links to your JDownloader-Downloadlist\r\n" +
                "!status -> You will get the Status of the last 10 Packages added to JDownloader\r\n" +
                "!ping -> I´ll send you a pong\r\n");
        }
    }
}