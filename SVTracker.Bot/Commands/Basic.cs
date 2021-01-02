using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System.Threading.Tasks;

namespace SVTracker.Commands
{
    public class Basic : BaseCommandModule
    {
        [Command("ping")]
        [Aliases("p")]
        public async Task Ping(CommandContext ctx)
        {
            await ctx.Channel.SendMessageAsync($"Pong! {ctx.Client.Ping}ms").ConfigureAwait(false);
        }

        [Command("version")]
        [Aliases("ver", "v")]
        public async Task Version(CommandContext ctx)
        {
            await ctx.Channel.SendMessageAsync("This SVTracker is version 0.4").ConfigureAwait(false);
        }
    }
}
