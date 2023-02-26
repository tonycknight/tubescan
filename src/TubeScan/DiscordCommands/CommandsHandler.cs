using System.Diagnostics.CodeAnalysis;
using Discord.Commands;
using Discord.WebSocket;
using TubeScan.Telemetry;

namespace TubeScan.DiscordCommands
{
    [ExcludeFromCodeCoverage] // Excluded until Discord.Net provides complete interfaces
    internal class CommandsHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commandService;
        private readonly ITelemetry _telemetry;
        private readonly IServiceProvider _serviceProvider;

        public CommandsHandler(DiscordSocketClient client, CommandService commandService,
                                    ITelemetry telemetry, IServiceProvider serviceProvider)
        {
            _client = client;
            _commandService = commandService;
            _telemetry = telemetry;
            _serviceProvider = serviceProvider;
        }

        public async Task InstallCommandsAsync()
        {
            _client.MessageReceived += HandleCommandAsync;

            await _commandService.AddModuleAsync(type: typeof(AboutCommands), services: _serviceProvider);
            await _commandService.AddModuleAsync(type: typeof(LineCommands), services: _serviceProvider);
            await _commandService.AddModuleAsync(type: typeof(SessionCommands), services: _serviceProvider);
            await _commandService.AddModuleAsync(type: typeof(StationCommands), services: _serviceProvider);
        }

        public async Task HandleCommandAsync(SocketMessage arg)
        {
            var message = arg as SocketUserMessage;
            var argPos = 0;

            if (message == null ||
                message.Author.IsBot ||
                message.Author.IsWebhook)
            {
                return;
            }

            var context = new SocketCommandContext(_client, message);

            await _commandService.ExecuteAsync(context: context, argPos: argPos, services: _serviceProvider);
        }
    }
}
