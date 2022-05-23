using System.Diagnostics.CodeAnalysis;
using Tk.Extensions;
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
        private readonly ILineProvider _lineProvider;
        private readonly IUsersRepository _usersRepo;
        private IList<LineStatus>? _lastKnownLineStatuses;

        public LineStatusPollingJob(ITelemetry telemetry, IDiscordProxy discordProxy, ILineProvider lineProvider, IUsersRepository usersRepo)
        {
            _telemetry = telemetry;
            _discordProxy = discordProxy;
            _lineProvider = lineProvider;
            _usersRepo = usersRepo;
        }

        public TimeSpan Frequency => TimeSpan.FromSeconds(60);

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
                    await BroadcastToUsersAsync(lines, newStatuses);
                }
            }

            _lastKnownLineStatuses = newLineStatuses;
        }

        private async Task BroadcastToUsersAsync(Dictionary<string, Line> lines, Dictionary<string, LineStatus> newStatuses)
        {
            _telemetry.Message($"Found {newStatuses.Count} new line statuses.");

            var users = await _usersRepo.GetAllUsersAsync();            
            if (users.Count > 0)
            {
                var userIds = users.Select(u => u.Id);
                var discordUsers = await _discordProxy.GetUsersAsync(userIds);

                _telemetry.Message($"Broadcasting to {users.Count} users...");
                foreach (var newStatus in newStatuses)
                {
                    var line = lines.GetOrDefault(newStatus.Key);
                    await BroadcastLineStatusAsync(discordUsers, line, newStatus.Value);
                }
            }
            _telemetry.Message($"Broadcasted to {users.Count} users.");
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
                    _telemetry.Error(ex.Message);
                }
            }
        }
    }
}
