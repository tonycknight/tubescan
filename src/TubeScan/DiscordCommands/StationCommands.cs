using System.Diagnostics.CodeAnalysis;
using Discord.Commands;
using TubeScan.Lines;
using TubeScan.Search;
using TubeScan.Stations;
using TubeScan.Telemetry;

namespace TubeScan.DiscordCommands
{
    [ExcludeFromCodeCoverage] // Excluded until Discord.Net provides complete interfaces
    internal class StationCommands : ModuleBase<SocketCommandContext>
    {
        private readonly ITelemetry _telemetry;
        private readonly ILineProvider _lineProvider;
        private readonly IStationProvider _stationProvider;
        private readonly IStationTagRepository _tagRepo;

        public StationCommands(ITelemetry telemetry, 
                                Lines.ILineProvider lineProvider,
                                IStationProvider stationProvider, IStationTagRepository tagRepo)
        {
            _telemetry = telemetry;
            _lineProvider = lineProvider;
            _stationProvider = stationProvider;
            _tagRepo = tagRepo;
        }

        [Command("tag", RunMode = RunMode.Async)]
        [System.ComponentModel.Description("Set a station name tag. Form: ``tag <tag name> <station name>``.")]
        public async Task SetStationTagAsync(string tagName, [Remainder] string stationNameQuery)
        {
            try
            {
                var authorId = Context.GetAuthorId();
                
                var stations = await _stationProvider.GetStationsAsync();

                var matches = stations.Match(stationNameQuery, s => s.ShortName).ToList();
                if (!matches.Any())
                {
                    ReplyAsync("None found.");
                    return;
                }

                var station = matches.First();

                await _tagRepo.SetAsync(authorId, new Models.StationTag(station.Value.NaptanId, tagName));

                ReplyAsync($"Done. Set ``{tagName}`` for ``{station.Value.ShortName}``.");
            }
            catch (Exception ex)
            {
                ex.ToString().CreateTelemetryEvent(TelemetryEventKind.Error).Send(_telemetry);
                ReplyAsync(ex.Message);
            }
        }

        [Command("-tag", RunMode = RunMode.Async)]
        [System.ComponentModel.Description("Remove a station name tag. Form: ``-tag <tag name>``.")]
        public async Task RemoveStationTagAsyncd(string tagName)
        {
            try
            {
                var authorId = Context.GetAuthorId();

                await _tagRepo.RemoveAsync(authorId, tagName);

                ReplyAsync($"Done.");
            }
            catch (Exception ex)
            {
                ex.ToString().CreateTelemetryEvent(TelemetryEventKind.Error).Send(_telemetry);
                ReplyAsync(ex.Message);
            }
        }

        [Command("tags", RunMode = RunMode.Async)]
        [System.ComponentModel.Description("List all your tags")]
        public async Task GetStationTagsAsync()
        {
            try
            {
                var authorId = Context.GetAuthorId();
                
                var tags = await _tagRepo.GetAllAsync(authorId);
                if(tags.Count == 0)
                {
                    ReplyAsync("None found.");
                }

                var stations = (await _stationProvider.GetStationsAsync());

                var msg = tags.RenderStationTags(stations);

                ReplyAsync(msg);

            }
            catch (Exception ex)
            {
                ex.ToString().CreateTelemetryEvent(TelemetryEventKind.Error).Send(_telemetry);
                ReplyAsync(ex.Message);
            }
        }

        [Command("station", RunMode = RunMode.Async)]
        [Alias("stn", "s")]
        [System.ComponentModel.Description("Get a station's status. Form: ``station <tag>``.")]
        public async Task GetStationStatusAsync(string tag)
        {
            try
            {
                var responseMsg = await ReplyAsync(":thinking: *Thinking...*");

                var authorId = Context.GetAuthorId();

                var stationTag = await _tagRepo.GetAsync(authorId, tag);

                if(stationTag == null)
                {
                    await responseMsg.ModifyAsync(mp =>
                    {
                        mp.Content = "Tag not found.";
                    });
                    return;
                }
                var stations = (await _stationProvider.GetStationsAsync());
                var station = stations.FirstOrDefault(s => s.NaptanId == stationTag.NaptanId);
                if(station == null)
                {
                    await responseMsg.ModifyAsync(mp =>
                    {
                        mp.Content = "Station not found.";
                    });
                    return;
                }
                                
                var status = await _stationProvider.GetStationStatusAsync(station.NaptanId);

                var lines = await _lineProvider.GetLinesAsync();
                Func<string, string> lineName = id => lines.FirstOrDefault(l => StringComparer.InvariantCultureIgnoreCase.Equals(l.Id, id))?.Name;
                Func<string, string> stationName = id => stations.FirstOrDefault(s => StringComparer.InvariantCultureIgnoreCase.Equals(s.NaptanId, id))?.ShortName;

                var eb = station.RenderStationStatus(status, lineName, stationName);
                responseMsg.ModifyAsync(mp =>
                {
                    mp.Content = "";
                    mp.Embed = eb;
                });

            }
            catch (Exception ex)
            {
                ex.ToString().CreateTelemetryEvent(TelemetryEventKind.Error).Send(_telemetry);
                ReplyAsync(ex.Message);
            }            
        }


    }
}
