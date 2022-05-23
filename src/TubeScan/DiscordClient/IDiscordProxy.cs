using Discord;
using Discord.WebSocket;

namespace TubeScan.DiscordClient
{
    internal interface IDiscordProxy : IDisposable
    {
        string ClientId { get; }
        string BotRegistrationUri { get; }
        public DiscordSocketClient Client { get; }
        Task StartAsync();
        Task StopAsync();
        void AddMessageReceivedHandler(Func<SocketUserMessage, Task> handler);
        IEnumerable<IMessageChannel> GetDmChannels();
        IEnumerable<IMessageChannel> GetChannels();
    }
}
