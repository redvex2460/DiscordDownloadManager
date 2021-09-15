using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DownloadManager.Bot.Discord.Workflow
{
    //class WorkflowModule : IDisposable
    //{
    //    protected SocketCommandContext _context;
    //    private Dictionary<string, SocketCallback<object>> _callbacks = new Dictionary<string, SocketCallback<object>>();
    //    private readonly DiscordSocketClient _discordBot;
    //    public WorkflowModule(SocketCommandContext context, DiscordSocketClient discordBot)
    //    {
    //        _context = context;
    //        _discordBot = discordBot;
    //        _discordBot.MessageReceived += OnMessageReceivedCallback;
    //        //_discordBot.ReactionAdded += OnReactionAddedCallback;

    //    }

    //    //private Task OnReactionAddedCallback(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3)
    //    //{
    //    //}

    //    private async Task OnMessageReceivedCallback(SocketMessage arg)
    //    {
    //        if (arg.Author.Id == _discordBot.CurrentUser.Id) return;
    //        if (arg.Reference == null) return;
    //        if (!(_callbacks.TryGetValue($"{arg.Reference.MessageId.Value}.message", out var callback))) return;
    //        if (!(callback.Context.Message.Author.Id == arg.Author.Id && callback.Context.Channel.Id == arg.Channel.Id)) return;

    //        switch(callback.RunMode)
    //        {
    //            case RunMode.Async:
    //                Task.Run(async () =>
    //                {
    //                    if (await callback.HandleCallbackAsync(arg).ConfigureAwait(false))
    //                        RemoveCallbacks(arg);
    //                });
    //                break;
    //            default:
    //                if (await callback.HandleCallbackAsync(arg).ConfigureAwait(false))
    //                    RemoveCallbacks(arg);
    //                break;
    //        }
    //    }

    //    public async Task<object> WaitForReaction(SocketCommandContext context, IMessage message, IEmote emote)
    //    {
    //        var token = new CancellationToken();
    //        var timeout = TimeSpan.FromSeconds(60);

    //        var eventTrigger = new TaskCompletionSource<SocketReaction>();
    //        var messageTrigger = new TaskCompletionSource<string>();
    //        var cancelTrigger = new TaskCompletionSource<bool>();

    //        token.Register(() => cancelTrigger.SetResult(true));

    //        var sourceContext = context;

    //        AddReactionCallback<SocketReaction>(message, new AsyncReactionCallback(sourceContext, reaction =>
    //        {
    //            if(reaction.Emote.Name.Equals(emote.Name, StringComparison.Ordinal))
    //            {
    //                eventTrigger.SetResult(reaction);
    //                return Task.FromResult(true);
    //            }
    //            return Task.FromResult(false);
    //        }));

    //        AddMessageCallback<SocketMessage>(message, new AsyncMessageCallback(sourceContext, reaction =>
    //        {
    //            if(reaction.Reference.MessageId.Value == sourceContext.Message.Id)
    //            {
    //                messageTrigger.SetResult(reaction.Content);
    //                return Task.FromResult(true);
    //            }
    //            return Task.FromResult(false);
    //        }));

    //        try
    //        {
    //            var trigger = eventTrigger.Task;
    //            var cancel = cancelTrigger.Task;
    //            var messagetask = messageTrigger.Task;
    //            var delay = Task.Delay(timeout);
    //            var task = await Task.WhenAny(trigger, cancel, messagetask, delay);

    //            if (task == trigger)
    //            {
    //                return await trigger.ConfigureAwait(false);
    //            }
    //            else if (task == messagetask)
    //            {
    //                return await messagetask.ConfigureAwait(false);
    //            }
    //            else
    //                return null;
    //        }
    //        catch
    //        {
    //            return null;
    //        }
    //        finally
    //        {
    //            RemoveCallbacks(message);
    //        }
    //    }

    //    private void RemoveCallbacks(IMessage message)
    //    {
    //        _callbacks.Remove($"{message.Id}.reaction");
    //        _callbacks.Remove($"{message.Id}.message");
    //    }

    //    public async Task<IUserMessage> ReplyToUserAsync(string message)
    //    {
    //        return await _context.Channel.SendMessageAsync($"{_context.User.Mention} -> {message}");
    //    }

    //    public void AddMessageCallback<T>(IMessage message, AsyncMessageCallback socketCallback) => _callbacks[$"{message.Id}.message"] = (SocketCallback<object>)socketCallback;
    //    public void AddReactionCallback<T>(IMessage message, AsyncReactionCallback socketCallback) => _callbacks[$"{message.Id}.reaction"] = (SocketCallback<object>)socketCallback;

    //    public void Dispose()
    //    {
    //        _discordBot.MessageReceived -= OnMessageReceivedCallback;
    //    }
    //}
}
