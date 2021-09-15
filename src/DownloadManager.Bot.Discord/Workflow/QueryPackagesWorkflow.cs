using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DownloadManager.Downloader.JDownloader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DownloadManager.Bot.Discord.Workflow
{
    class QueryPackagesWorkflow : WorkflowModule
    {
        SocketUser _user { get { return _context.User; } }
        ISocketMessageChannel _channel { get { return _context.Channel; } }
        IUserMessage _lastBotMessage;
        public QueryPackagesWorkflow(SocketCommandContext context, DiscordSocketClient discordBot) : base(context, discordBot)
        {
        }

        internal async Task HandleQueryPackagesRequestAsync()
        {
            if (_user.IsBot) return;

            if(!Utils.UserHasDownloadManagerRole(_user))
            {
                await ReplyToUserAsync("Sorry you have insufficient rights to use this bot, please contact the host!");
                return;
            }

            await DoTheRequest();
        }

        private async Task DoTheRequest()
        {
            var cancelEmoji = new Emoji("\u26D4");
            await _channel.SendMessageAsync("I´m querying now the API, please have a seat, I get your informations, this could take a while.");
            _lastBotMessage = await _channel.SendMessageAsync("If you want to cancel your request click on that cancel button below this Message!");
            await _lastBotMessage?.AddReactionAsync(cancelEmoji); 
            var reaction = WaitForReaction(_context, _lastBotMessage, cancelEmoji);
            var apiResponse = Api.Instance.QueryLinks();
            var doneTask = Task.WhenAny(reaction, apiResponse);
            if (doneTask.Result == reaction)
            {
                if (reaction.Result.Emote == cancelEmoji)
                {
                    await ReplyToUserAsync("Okay you canceld your request!");
                    return;
                }
            }
            else if(doneTask.Result == apiResponse)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("Current status of JDownloader:");
                foreach (var item in apiResponse.Result)
                {
                    string finishedString = "not finished";
                    if (item.Finished) finishedString = "finished";
                    sb.AppendLine($"{item.Name} - {finishedString}");
                }
                await _context.Channel.SendMessageAsync(sb.ToString());
            }
        }
    }
}
