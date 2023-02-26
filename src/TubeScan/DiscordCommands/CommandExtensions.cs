using Discord.Commands;

namespace TubeScan.DiscordCommands
{
    internal static class CommandExtensions
    {
        public static bool IsDMChannel(this SocketCommandContext context)
            => (context.Channel as Discord.WebSocket.SocketDMChannel) != null;

        public static ulong GetAuthorId(this SocketCommandContext context)
            => context.Message.Author.Id;

        public static string GetAuthorMention(this SocketCommandContext context)
            => context.Message.Author.Mention;
    }
}
