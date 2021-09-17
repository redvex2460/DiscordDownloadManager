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

namespace DownloadManager.Bot.DiscordBot
{
    public class DiscordBot
    {
        public Task BotTask { get; set; }
        DiscordSocketClient Client { get; set; }
        public string Token { get; set; }

        public DiscordBot(string token)
        {
            Client = new DiscordSocketClient();
            Client.Log += Log;
            Client.GuildAvailable += OnGuildAvailable;
            Client.SlashCommandExecuted += OnSlashCommand;
            Token = token;
            BotTask = Task.Run(StartBot);
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
                        var autodownload = arg.Data.Options.FirstOrDefault(a => a.Name.Equals("autodownload")) != null ? (bool)arg.Data.Options.FirstOrDefault(a => a.Name.Equals("autodownload")).Value : (bool)SettingsFile.CachedSettings.JDownloader.AutoDownload;
                        if (Api.Instance.AddDownloadLink(links, name, autodownload:autodownload))
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
                            if (arg.Data.Options != null && (bool)arg.Data.Options.FirstOrDefault(option => option.Name == "finished").Value == false)
                                result = result.Where(item => !item.Status.ToLower().Contains("finished")).ToList();
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
