using System.Diagnostics.CodeAnalysis;
using Tk.Extensions.Collections;
using Tk.Extensions.Linq;
using TubeScan.DiscordClient;
using TubeScan.DiscordCommands;
using TubeScan.Models;
using TubeScan.Scheduling;
using TubeScan.Telemetry;
using TubeScan.Users;

namespace TubeScan.Lines
{
    [ExcludeFromCodeCoverage] // Taking the easy way out...
    internal class LineStatusPollingJob : IJob
    {
        private readonly ITelemetry _telemetry;
        private readonly IDiscordProxy _discordProxy;
        private readonly ISettable<LineStatus> _lineSettable;
        private readonly ILineReferenceProvider _lineReferenceProvider;
        private readonly ILineStatusProvider _lineProvider;
        private readonly IUsersRepository _usersRepo;
        private IList<LineStatus>? _lastKnownLineStatuses;

        public LineStatusPollingJob(ITelemetry telemetry, IDiscordProxy discordProxy,
                                    ISettable<Models.LineStatus> lineSettable,
                                    ILineReferenceProvider lineReferenceProvider,
                                    ILineStatusProvider lineProvider, IUsersRepository usersRepo)
        {
            _telemetry = telemetry;
            _discordProxy = discordProxy;
            _lineSettable = lineSettable;
            _lineReferenceProvider = lineReferenceProvider;
            _lineProvider = lineProvider;
            _usersRepo = usersRepo;
        }

        public TimeSpan Frequency => TimeSpan.FromSeconds(60);

        public async Task ExecuteAsync()
        {
            var newLineStatuses = await _lineProvider.GetLineStatusAsync();

            $"Received {newLineStatuses.Count} line statuses from TFL.".CreateTelemetryEvent().Send(_telemetry);

            if (_lastKnownLineStatuses != null && newLineStatuses.NullToEmpty().Any())
            {
                var lines = _lineReferenceProvider.GetLines().NullToEmpty().ToDictionary(s => s.Id);
                var newStatuses = _lastKnownLineStatuses.GetDeltas(newLineStatuses).ToDictionary(s => s.Id);
                if (newStatuses.Any())
                {
                    await BroadcastToUsersAsync(lines, newStatuses);
                }
            }
            _lineSettable.Set(newLineStatuses);
            _lastKnownLineStatuses = newLineStatuses;
        }

        private async Task BroadcastToUsersAsync(Dictionary<string, Line> lines, Dictionary<string, LineStatus> newStatuses)
        {
            $"Found {newStatuses.Count} new line statuses.".CreateTelemetryEvent().Send(_telemetry);

            var users = await _usersRepo.GetAllUsersAsync();
            if (users.Count > 0)
            {
                var userIds = users.Select(u => u.Id);
                var discordUsers = await _discordProxy.GetUsersAsync(userIds);

                $"Broadcasting to {users.Count} user(s)...".CreateTelemetryEvent().Send(_telemetry);
                foreach (var newStatus in newStatuses)
                {
                    var line = lines.GetOrDefault(newStatus.Key);
                    await BroadcastLineStatusAsync(discordUsers, line, newStatus.Value);
                }
            }
            $"Broadcasted to {users.Count} user(s).".CreateTelemetryEvent().Send(_telemetry);
        }

        private async Task BroadcastLineStatusAsync(IEnumerable<Discord.IUser> users, Line line, LineStatus lineStatus)
        {
            var newStatusLookup = lineStatus.Singleton().ToDictionary(s => s.Id);
            var renderedStatus = line.Singleton().RenderLinesStatus(newStatusLookup, true);

            foreach (var user in users)
            {
                try
                {
                    var channel = await user.CreateDMChannelAsync();
                    if (channel != null)
                    {
                        await channel.SendMessageAsync(renderedStatus);
                    }
                }
                catch (Exception ex)
                {
                    ex.Message.CreateTelemetryEvent(TelemetryEventKind.Error).Send(_telemetry);
                }
            }
        }
    }
}
