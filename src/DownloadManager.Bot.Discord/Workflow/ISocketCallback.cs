using Discord.Commands;
using System.Threading.Tasks;

namespace DownloadManager.Bot.Discord.Workflow
{
    public interface ISocketCallback<T>
    {
        RunMode RunMode { get; }
        SocketCommandContext Context { get; }

        Task<bool> HandleCallbackAsync(T reaction);
    }
}