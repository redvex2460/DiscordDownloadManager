using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DownloadManager.Downloader.JDownloader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DownloadManager.Bot.Discord.Workflow
{
    class AddDownloadsWorkflow : WorkflowModule
    {
        /* -> Bot sends Context Message to Workflow
         * -> Extract Links from Context Message
         * -> Ask if user wants to add a Name to download
         * -> Build API Object
         * -> Send Api Object
         * -> Reply if addlink succeeded
         */
        SocketUser _user { get { return _context.User; } }
        ISocketMessageChannel _channel { get { return _context.Channel; } }
        IUserMessage _lastBotMessage;

        public AddDownloadsWorkflow(SocketCommandContext context, DiscordSocketClient discordBot) : base(context, discordBot)
        {
        }

        public async Task HandleDownloadRequestAsync()
        {
            //If user is a bot, drop request;
            if (_user.IsBot) return;

            //If user has insufficient rights, drop request;
            if (!Utils.UserHasDownloadManagerRole(_user)) return;

            var links = ExtractLinks();

            if(string.IsNullOrEmpty(links))
            {
                await _channel.SendMessageAsync("No links could be find, please check your command and retry.");
                return;
            }

            await RequestLinksAsync(links);
        }

        private async Task RequestLinksAsync(string links)
        {
            _lastBotMessage = await _channel.SendMessageAsync("Do you want to add a Name for your Package then reply to this message, otherwise click on the reaction below this message!");
            await _lastBotMessage?.AddReactionAsync(new Emoji("\u23EC"));
            var reaction = WaitForReaction(_context, _lastBotMessage, new Emoji("\u23EC"));
            var msg = WaitForMessage(_context, _lastBotMessage);
            var waitResult = Task.WhenAny(msg, reaction);
            if (waitResult.Result == reaction)
                Api.Instance.AddDownloadLink(links);
            else if (waitResult.Result == msg)
                Api.Instance.AddDownloadLink(links, msg.Result.ToString());
        }

        private string ExtractLinks()
        {
            string urlPattern = @"(?<Links>https?\:\/\/[Aa0-zZ9.\/-]*)";
            StringBuilder sb = new StringBuilder();
            var matches = Regex.Matches(_context.Message.Content, urlPattern);
            foreach (var match in matches)
            {
                sb.AppendLine(match.ToString());
            }
            return sb.ToString();
        }
    }
}
