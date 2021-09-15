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
    class WorkflowModule : IDisposable
    {
        protected SocketCommandContext _context;
        private Dictionary<ulong, AsyncMessageCallback> _messageCallbacks = new Dictionary<ulong, AsyncMessageCallback>();
        private Dictionary<ulong, AsyncReactionCallback> _reactionCallbacks = new Dictionary<ulong, AsyncReactionCallback>();
        private readonly DiscordSocketClient _discordBot;
        public WorkflowModule(SocketCommandContext context, DiscordSocketClient discordBot)
        {
            _context = context;
            _discordBot = discordBot;
            _discordBot.MessageReceived += OnMessageReceivedCallback;
            _discordBot.ReactionAdded += OnReactionAddedCallback;

        }

        private async Task OnReactionAddedCallback(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel, SocketReaction reaction)
        {
            if (reaction.UserId == _discordBot.CurrentUser.Id) return;
            if (!(_reactionCallbacks.TryGetValue(message.Id, out var callback))) return;
            if (!(callback.Context.Message.Author.Id == reaction.UserId && callback.Context.Channel.Id == reaction.Channel.Id)) return;

            switch (callback.RunMode)
            {
                case RunMode.Async:
                    _ = Task.Run(async () =>
                    {
                        if (await callback.HandleCallbackAsync(reaction).ConfigureAwait(false))
                            RemoveReactionCallback(message.Value);
                    });
                    break;
                default:
                    if (await callback.HandleCallbackAsync(reaction).ConfigureAwait(false))
                        RemoveReactionCallback(message.Value);
                    break;
            }
        }

        private async Task OnMessageReceivedCallback(SocketMessage arg)
        {
            if (arg.Author.Id == _discordBot.CurrentUser.Id) return;
            if (arg.Reference == null) return;
            if (!(_messageCallbacks.TryGetValue(arg.Reference.MessageId.Value, out var callback))) return;
            if (!(callback.Context.Message.Author.Id == arg.Author.Id && callback.Context.Channel.Id == arg.Channel.Id)) return;

            switch (callback.RunMode)
            {
                case RunMode.Async:
                    _ = Task.Run(async () =>
                    {
                        if (await callback.HandleCallbackAsync(arg).ConfigureAwait(false))
                            RemoveMessageCallback(arg);
                    });
                    break;
                default:
                    if (await callback.HandleCallbackAsync(arg).ConfigureAwait(false))
                        RemoveMessageCallback(arg);
                    break;
            }
        }

        public async Task<SocketReaction> WaitForReaction(SocketCommandContext context, IMessage message, IEmote emote)
        {
            var eventTrigger = new TaskCompletionSource<SocketReaction>();
            var sourceContext = context;

            AddReactionCallback(message, new AsyncReactionCallback(sourceContext, reaction =>
            {
                if (reaction.Emote.Name.Equals(emote.Name, StringComparison.Ordinal))
                {
                    eventTrigger.SetResult(reaction);
                    return Task.FromResult(true);
                }
                return Task.FromResult(false);
            }));

            try
            {
                var trigger = eventTrigger.Task;
                var task = await Task.WhenAny(trigger);

                if (task == trigger)
                {
                    return await trigger.ConfigureAwait(false);
                }
                else
                    return null;
            }
            catch
            {
                return null;
            }
            finally
            {
                RemoveReactionCallback(message);
            }
        }

        public async Task<bool> WaitForMessage(SocketCommandContext context, IMessage message, string awaitedString)
        {
            var triggerResult = new TaskCompletionSource<bool>();
            var socketContext = context;

            AddMessageCallback(message, new AsyncMessageCallback(socketContext, obj =>
            {
                if(obj.Content == awaitedString)
                {
                    triggerResult.SetResult(true);
                    return Task.FromResult(true);
                }
                return Task.FromResult(false);
            }));

            try
            {
                var trigger = triggerResult.Task;
                var task = await Task.WhenAny(trigger);

                return await trigger.ConfigureAwait(false);
            }
            catch
            {
                return false;
            }
            finally
            {
                RemoveMessageCallback(message);
            }
        }
        public async Task<IUserMessage> ReplyToUserAsync(string message)
        {
            return await _context.Channel.SendMessageAsync($"{_context.User.Mention} -> {message}");
        }

        private void RemoveMessageCallback(IMessage message)
        {
            if (message == null) return;
            if (_messageCallbacks.ContainsKey(message.Id))
                _messageCallbacks.Remove(message.Id);
        }
        private void RemoveReactionCallback(IMessage message)
        {
            if (message == null) return;
            if (_reactionCallbacks.ContainsKey(message.Id))
                _reactionCallbacks.Remove(message.Id);
        }
        public void AddMessageCallback(IMessage message, AsyncMessageCallback socketCallback) => _messageCallbacks[message.Id] = socketCallback;
        public void AddReactionCallback(IMessage message, AsyncReactionCallback socketCallback) => _reactionCallbacks[message.Id] = socketCallback;

        public void Dispose()
        {
            _discordBot.MessageReceived -= OnMessageReceivedCallback;
        }
    }
}
