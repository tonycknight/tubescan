using System.Diagnostics.CodeAnalysis;
using Tk.Extensions;
using TubeScan.DiscordClient;
using TubeScan.DiscordCommands;
using TubeScan.Models;
using TubeScan.Scheduling;
using TubeScan.Telemetry;

namespace TubeScan.Lines
{
    [ExcludeFromCodeCoverage] // Taking the easy way out...
    internal class LineStatusPollingJob : IJob
    {
        private readonly ITelemetry _telemetry;
        private readonly IDiscordProxy _discordProxy;
        private readonly ILineProvider _lineProvider;
        private IList<LineStatus>? _lastKnownLineStatuses;

        public LineStatusPollingJob(ITelemetry telemetry, IDiscordProxy discordProxy, ILineProvider lineProvider)
        {
            _telemetry = telemetry;
            _discordProxy = discordProxy;
            _lineProvider = lineProvider;
        }

        public TimeSpan Frequency => TimeSpan.FromSeconds(30);

        public async Task ExecuteAsync()
        {
            var newLineStatuses = await _lineProvider.GetLineStatusAsync();

            _telemetry.Message($"Received {newLineStatuses.Count} line statuses from TFL.");

            if (_lastKnownLineStatuses != null && newLineStatuses.NullToEmpty().Any())
            {
                var lines = (await _lineProvider.GetLinesAsync()).NullToEmpty().ToDictionary(s => s.Id);
                var newStatuses = _lastKnownLineStatuses.GetDeltas(newLineStatuses).ToDictionary(s => s.Id);
                if (newStatuses.Any())
                {
                    await BroadcastToChannelsAsync(lines, newStatuses);
                }
            }

            _lastKnownLineStatuses = newLineStatuses;
        }

        private async Task BroadcastToChannelsAsync(Dictionary<string, Line> lines, Dictionary<string, LineStatus> newStatuses)
        {
            _telemetry.Message($"Found {newStatuses.Count} new line statuses.");
            var channels = _discordProxy.GetChannels().ToList();
            if (channels.Count > 0)
            {
                _telemetry.Message($"Broadcasting to {channels.Count} channels...");
                foreach (var newStatus in newStatuses)
                {
                    var line = lines.GetOrDefault(newStatus.Key);
                    await BroadcastLineStatusAsync(channels, line, newStatus.Value);
                }
            }
            _telemetry.Message($"Broadcasted to {channels.Count} channels.");
        }

        private async Task BroadcastLineStatusAsync(IEnumerable<Discord.IMessageChannel> channels, Line line, LineStatus lineStatus)
        {
            var newStatusLookup = lineStatus.Singleton().ToDictionary(s => s.Id);
            var renderedStatus = line.Singleton().RenderLinesStatus(newStatusLookup, true);

            foreach (var channel in channels)
            {
                await channel.SendMessageAsync(renderedStatus);
            }
        }
    }
}
