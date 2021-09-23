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
using DownloadManager.Bot.DiscordBot.Commands;
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
                
                slashCommandBuilder = new();
                slashCommandBuilder.WithName("permit");
                slashCommandBuilder.AddOption("user", ApplicationCommandOptionType.User, "User which should be added to Downloadmanager");
                slashCommandBuilder.WithDescription("Adds the selected user to DownloadManagerRole");
                await arg.CreateApplicationCommandAsync(slashCommandBuilder.Build());

                await SetupServerRoles(arg);
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
                        await DownloadCommand.HandleCommand(arg);
                        break;
                    case "status":
                        await StatusCommand.HandleCommand(arg);
                        break;
                    case "permit":
                        await PermitCommand.HandleCommand(arg);
                        break;
                }
            });
            return Task.CompletedTask;
        }

        private async Task SetupServerRoles(IGuild server)
        {
            var role = server.GetRole(SettingsFile.CachedSettings.Discord.Role);
            if(role == null)
            {
                Logger.LogMessage("Discord: Cant find needed Serverrole.");
                Logger.LogMessage("Discord: Creating now");
                role = await server.CreateRoleAsync(SettingsFile.CachedSettings.Discord.Role, GuildPermissions.None, Color.DarkBlue, false, null);
            }
        }

        #endregion Private Methods
    }
}
