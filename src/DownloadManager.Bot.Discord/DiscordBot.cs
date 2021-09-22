using System;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Net;
using Discord.Commands;
using DownloadManager.Core;
using DownloadManager.Core.Logging;
using DownloadManager.Downloader.JDownloader;
using Discord.WebSocket;
using DownloadManager.Bot.Data;
using DownloadManager.Downloader.JDownloader.Models;

namespace DownloadManager.Bot.DiscordBot
{
    public class DiscordBot
    {
        #region Public Constructors

        public DiscordBot(string token)
        {
            Client = new DiscordSocketClient();
            Client.Log += Log;
            Client.GuildAvailable += OnGuildAvailable;
            Client.SlashCommandExecuted += OnSlashCommand;
            Client.Ready += OnReady;
            Token = token;
            BotTask = Task.Run(StartBot);
        }

        private Task OnDownloadFinished(QueryPackagesServerResponse package)
        {
            _ = Task.Run(async () =>
            {
                IMessageChannel channel = (IMessageChannel)await Client.GetChannelAsync(887052383954292776);
                await channel.SendMessageAsync($"{package.Name} finished download!");
            });
            return Task.CompletedTask;
        }

        private Task OnReady()
        {
            if (Client.Guilds.Count <= 0)
                Logger.LogMessage("Looks like I´m not member of a Server, please add me to the Server");
            Api.Instance.OnDownloadFinished += OnDownloadFinished;
            return Task.CompletedTask;
        }

        #endregion Public Constructors

        #region Public Properties

        public Task BotTask { get; set; }
        public string Token { get; set; }

        #endregion Public Properties

        #region Private Properties
        DiscordSocketClient Client { get; set; }

        #endregion Private Properties

        #region Public Methods

        public async Task StartBot()
        {
            await Client.LoginAsync(TokenType.Bot, Token);
            await Client.StartAsync();
            await Task.Delay(-1);
        }

        #endregion Public Methods

        #region Private Methods

        private Task Log(LogMessage logMessage)
        {
            Logger.LogMessage($"Discord: {logMessage}");
            return Task.CompletedTask;
        }

        private Task OnGuildAvailable(SocketGuild arg)
        {
            _ = Task.Run(async () =>
            {
                SlashCommandBuilder slashCommandBuilder = new();
                slashCommandBuilder.WithName("download");
                slashCommandBuilder.WithDescription("Adds a Link / multiple Links to JDownloader");
                slashCommandBuilder.AddOption("links", ApplicationCommandOptionType.String, "Links");
                slashCommandBuilder.AddOption("name", ApplicationCommandOptionType.String, "Packagename", required: false);
                slashCommandBuilder.AddOption("autodownload", ApplicationCommandOptionType.String, "autodownload", required: false);
                await arg.CreateApplicationCommandAsync(slashCommandBuilder.Build());

                slashCommandBuilder = new();
                slashCommandBuilder.WithName("status");
                slashCommandBuilder.WithDescription("Query the JDownloader for active Downloads");
                slashCommandBuilder.AddOption("finished", ApplicationCommandOptionType.Boolean, "Should finished downloads be shown?", false);
                await arg.CreateApplicationCommandAsync(slashCommandBuilder.Build());
            });
            return Task.CompletedTask;
        }

        private Task OnSlashCommand(SocketSlashCommand arg)
        {
            _ = Task.Run(async () =>
            {
            switch (arg.Data.Name)
            {
                case "download":
                        if (!Utils.UserHasDownloadManagerRole(arg.User))
                        {
                            await arg.RespondAsync("You have insufficient rights, to use this command");
                            break;
                        }
                        var links = string.Join("\r\n",arg.Data.Options.FirstOrDefault(a => a.Name.Equals("links")).Value.ToString().Split(" "));
                        var name = arg.Data.Options.FirstOrDefault(a => a.Name.Equals("name")) != null ? arg.Data.Options.FirstOrDefault(a => a.Name.Equals("name")).Value.ToString() : "";
                        var autodownload = arg.Data.Options.FirstOrDefault(a => a.Name.Equals("autodownload")) != null ? (bool)arg.Data.Options.FirstOrDefault(a => a.Name.Equals("autodownload")).Value : SettingsFile.CachedSettings.JDownloader.AutoDownload;
                        if (await Api.Instance.AddDownloadLink(links, name, autodownload:autodownload))
                            await arg.RespondAsync("Worked!");
                        else
                            await arg.RespondAsync("There was an error!");
                        break;
                    case "status":
                        if (!Utils.UserHasDownloadManagerRole(arg.User))
                        {
                            await arg.RespondAsync("You have insufficient rights, to use this command");
                            break;
                        }
                        var result = await Api.Instance.QueryLinks();
                        if (result != null)
                        {
                            StringBuilder sb = new StringBuilder();
                            if (result.Count <= 0)
                            {
                                await arg.RespondAsync("No Downloads in queue");
                                break;
                            }
                            if (arg.Data.Options != null && (bool)arg.Data.Options.FirstOrDefault(option => option.Name == "finished").Value == false)
                                result = (ActivePackageList)result.Where(item => !item.Status.ToLower().Contains("finished")).ToList();
                            foreach (var obj in result)
                            {
                                sb.AppendLine($"{obj.Name} : status: {obj.Status}");
                            }
                            await arg.RespondAsync(sb.ToString());
                        }
                        else
                            await arg.RespondAsync("There was an error!");
                        break;
                }
            });
            return Task.CompletedTask;
        }

        #endregion Private Methods
    }
}
