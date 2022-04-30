using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using Discord;
using Discord.WebSocket;
using TubeScan.Config;
using TubeScan.Telemetry;

namespace TubeScan.DiscordClient
{
    [ExcludeFromCodeCoverage]
    internal class DiscordProxy : IDiscordProxy
    {        
        private readonly IAppConfigurationProvider _configProvider;
        private readonly ITelemetry _telemetry;
        private readonly ConcurrentBag<Func<SocketUserMessage, Task>> _messageReceivedHandlers;
        private DiscordSocketClient? _client;
        private readonly ulong _requiredPermissions;

        public DiscordProxy(IAppConfigurationProvider configProvider, ITelemetry telemetry)
        {
            _configProvider = configProvider;
            _telemetry = telemetry;
            _messageReceivedHandlers = new ConcurrentBag<Func<SocketUserMessage, Task>>();
            _requiredPermissions = 103079331840;

            var clientConfig = new DiscordSocketConfig()
            {
                AlwaysDownloadUsers = true,
            };

            _client = new DiscordSocketClient(clientConfig);

            _client.Log += client_Log;
            _client.Ready += client_Ready;
            _client.Disconnected += client_Disconnected;
            _client.MessageReceived += client_MessageReceived;
        }

        ~DiscordProxy()
        {
            Dispose(false);
        }

        public string ClientId => _configProvider.GetAppConfiguration().Discord.ClientId;

        public string BotRegistrationUri => $"https://discord.com/api/oauth2/authorize?client_id={ClientId}&permissions={_requiredPermissions}&scope=bot";

        public DiscordSocketClient Client => _client;

        public async Task StartAsync()
        {
            var config = _configProvider.GetAppConfiguration().Discord;

#pragma warning disable CS8602 // Dereference of a possibly null reference.
            await _client.LoginAsync(Discord.TokenType.Bot, config.ClientToken);
            await _client.StartAsync();
#pragma warning restore CS8602 // Dereference of a possibly null reference.
        }

        public async Task StopAsync()
        {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            await _client.StopAsync();
#pragma warning restore CS8602 // Dereference of a possibly null reference.
        }

        public void Dispose() => Dispose(true);

        private void Dispose(bool disposing)
        {
            _client?.Dispose();
            _client = null;
        }

        public void AddMessageReceivedHandler(Func<SocketUserMessage, Task> handler)
        {
            _messageReceivedHandlers.Add(handler);
        }

        private Task client_Log(LogMessage arg)
        {
            _telemetry.Message(arg.Message);

            return Task.CompletedTask;
        }

        private Task client_Ready()
        {
            return Task.CompletedTask;
        }

        private Task client_Disconnected(Exception arg)
        {
            return Task.CompletedTask;
        }

        private Task client_MessageReceived(SocketMessage arg)
        {
            var msg = arg as SocketUserMessage;
            if (msg != null && !msg.Author.IsBot)
            {
                Task.Run(() => HandleMessageAsync(msg));
            }
            return Task.CompletedTask;
        }

        private Task HandleMessageAsync(SocketUserMessage msg)
        {
            LogMessageContent(msg);

            foreach (var h in _messageReceivedHandlers)
            {
                _ = Task.Run(() => h(msg));
            }

            return Task.CompletedTask;
        }

        private void LogMessageContent(SocketMessage msg)
        {
            if (_configProvider.GetAppConfiguration().Telemetry?.LogMessageContent == true)
            {
                var guildName = (msg.Channel as SocketTextChannel)?.Guild?.Name;
                var prefix = guildName != null
                    ? $"[{guildName}] [{msg.Channel.Name}]"
                    : $"[{msg.Channel.Name}]";

                var line = $"{prefix} [Message {msg.Id}] [{UserLogPrefix(msg.Author)}] {msg.Content}";
                _telemetry.Message(line);
            }
        }

        private string UserLogPrefix(IUser user) => $"{user.Id} {user.Username}#{user.Discriminator}";
    }
}
