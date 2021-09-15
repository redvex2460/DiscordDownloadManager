using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DownloadManager.Bot.Discord.Workflow
{
    class AsyncReactionCallback : ISocketCallback<SocketReaction>
    {
        private readonly SocketCommandContext _context;
        private readonly Func<SocketReaction, Task<bool>> _callback;


        public RunMode RunMode => RunMode.Async;

        public SocketCommandContext Context => _context;
        public AsyncReactionCallback(SocketCommandContext context, Func<SocketReaction, Task<bool>> callback)
        {
            _context = context;
            _callback = callback;
        }

        public Task<bool> HandleCallbackAsync(SocketReaction reaction)
        {
            return _callback(reaction);
        }
    }

}
