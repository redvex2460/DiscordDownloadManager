using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DownloadManager.Bot.Discord.Workflow
{
    class AsyncMessageCallback : ISocketCallback<SocketMessage>
    {
        private readonly SocketCommandContext _context;
        private readonly Func<SocketMessage, Task<bool>> _callback;

        public RunMode RunMode => RunMode.Async;

        public SocketCommandContext Context => _context;
        public AsyncMessageCallback(SocketCommandContext context, Func<SocketMessage, Task<bool>> callback)
        {
            _context = context;
            _callback = callback;
        }

        public Task<bool> HandleCallbackAsync(SocketMessage reaction)
        {
            return _callback(reaction);
        }
    }
}
