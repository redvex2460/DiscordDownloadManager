using System;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DownloadManager.Bot.Discord.Workflow;
using DownloadManager.Core;
using DownloadManager.Core.Logging;
using DownloadManager.Downloader.JDownloader;

namespace DownloadManager.Bot.Discord
{
    public class DiscordBot
    {
        public Task BotTask { get; set; }
        DiscordSocketClient Client { get; set; }
        CommandService Commands { get; set; }
        public string Token { get; set; }

        public DiscordBot(string token)
        {
            Client = new DiscordSocketClient();
            Commands = new CommandService();
            Client.Log += Log;
            Client.MessageReceived += OnMessageRecieved;
            Commands.AddModulesAsync(assembly: Assembly.GetExecutingAssembly(), null);
            Token = token;
            BotTask = Task.Run(StartBot);
        }

        private Task OnMessageRecieved(SocketMessage arg)
        {
            _ = Task.Run(async () =>
            {
                var message = arg as SocketUserMessage;
                var context = new SocketCommandContext(Client, message);
                if (context.User.IsBot) return;

                int argPos = 0;
                if (message.HasStringPrefix("!", ref argPos))
                {
                    if (context.Message.Content.ToLower().Contains("download"))
                    {
                        var workflow = new AddDownloadsWorkflow(context, Client);
                        await workflow.HandleDownloadRequestAsync();
                    }
                    if (context.Message.Content.ToLower().Contains("status"))
                    {
                        var workflow = new QueryPackagesWorkflow(context, Client);
                        await workflow.HandleQueryPackagesRequestAsync();
                    }
                    if (context.Message.Content.ToLower().Contains("help"))
                    {
                        var workflow = new QueryHelpWorkflow(context, Client);
                        await workflow.HandleQueryPackagesRequestAsync();
                    }
                    if (context.Message.Content.ToLower().Contains("ping"))
                    {
                        await HandlePingCommand(context);
                    }
                }
                return;
            });
            return Task.CompletedTask;
        }

        private Task HandlePingCommand(SocketCommandContext context)
        {
            if (Utils.UserHasDownloadManagerRole(context.User))
            {
                if (context.Message.Content.ToLower() == "!ping")
                {
                    Api.Instance.PingDevice();
                }
            }
            return Task.CompletedTask;
        }

        public async Task StartBot()
        {
            await Client.LoginAsync(TokenType.Bot, Token);
            await Client.StartAsync();
            await Task.Delay(-1);
        }

        private Task Log(LogMessage logMessage)
        {
            Logger.LogMessage($"Discord: {logMessage}");
            return Task.CompletedTask;
        }

    }
}
