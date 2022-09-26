using System.Diagnostics.CodeAnalysis;
using Discord.Commands;
using Tk.Extensions;
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

                var responseMsg = await ReplyAsync(RenderingExtensions.Thinking);
                var responseText = "";

                var stations = await _stationProvider.GetStationsAsync();

                var matches = stations.Match(stationNameQuery, s => s.ShortName).ToList();
                if (!matches.Any())
                {
                    responseText = "No station found.";
                }
                else
                {
                    var station = matches.First();

                    await _tagRepo.SetAsync(authorId, new Models.StationTag(station.Value.NaptanId, tagName));

                    responseText = $"Done. Tag ``{tagName}`` set for ``{station.Value.ShortName}``.";

                }
                responseMsg.ModifyAsync(mp => { mp.Content = responseText; });
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

                var responseMsg = await ReplyAsync(RenderingExtensions.Thinking);

                var deleted = await _tagRepo.RemoveAsync(authorId, tagName);
                
                var responseText = deleted ? "Done." : "The tag was not found.";

                responseMsg.ModifyAsync(mp => { mp.Content = responseText; });
            }
            catch (Exception ex)
            {
                ex.ToString().CreateTelemetryEvent(TelemetryEventKind.Error).Send(_telemetry);
                ReplyAsync(ex.Message);
            }
        }

        [Command("tags", ignoreExtraArgs: true, RunMode = RunMode.Async)]
        [System.ComponentModel.Description("List all your tags")]
        public async Task GetStationTagsAsync()
        {
            try
            {
                var authorId = Context.GetAuthorId();

                var responseMsg = await ReplyAsync(RenderingExtensions.Thinking);
                var responseText = "";

                var tags = await _tagRepo.GetAllAsync(authorId);
                if (tags.Count == 0)
                {
                    responseText = "No tags found.";
                }
                else
                {
                    var stations = (await _stationProvider.GetStationsAsync());

                    responseText = tags.RenderStationTags(stations);
                }
                responseMsg.ModifyAsync(mp => { mp.Content = responseText; });                
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
                var authorId = Context.GetAuthorId();
                var responseMsg = await ReplyAsync(RenderingExtensions.Thinking);
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
                                
                var stationStatus = await _stationProvider.GetStationStatusAsync(station.NaptanId);

                var allLinesStatuses = await _lineProvider.GetLineStatusAsync();
                var stationLineIds = station.Lines.Select(sl => sl.Id).ToHashSet();
                var lineStatuses = allLinesStatuses.Where(ls => stationLineIds.Contains(ls.Id)).ToList();

                var lines = await _lineProvider.GetLinesAsync();
                Func<string, string> lineName = id => lines.FirstOrDefault(l => StringComparer.InvariantCultureIgnoreCase.Equals(l.Id, id))?.Name;
                Func<string, string> stationName = id => stations.FirstOrDefault(s => StringComparer.InvariantCultureIgnoreCase.Equals(s.NaptanId, id))?.ShortName;

                var eb = station.RenderStationStatus(stationStatus, lineName, lineStatuses, stationName);
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

        [Command("find", RunMode = RunMode.Async)]
        [System.ComponentModel.Description("Find a station. Form: ``find <station name>``.")]
        public async Task FindStationAsync([Remainder] string stationNameQuery)
        {
            try
            {                
                var responseMsg = await ReplyAsync(RenderingExtensions.Thinking);
                var responseText = "";
                                
                var matches = (await _stationProvider.GetStationsAsync())
                                      .Match(stationNameQuery, s => s.ShortName)                                      
                                      .Take(5).ToList();
                if (!matches.Any())
                {
                    responseText = "No station found.";
                }
                else
                {
                    var stationNames = matches.Select(si => $"``{si.Value.ShortName}``");
                    responseText = $"Found stations:{Environment.NewLine}{stationNames.Join(Environment.NewLine)}";
                }
                responseMsg.ModifyAsync(mp => { mp.Content = responseText; });
            }
            catch (Exception ex)
            {
                ex.ToString().CreateTelemetryEvent(TelemetryEventKind.Error).Send(_telemetry);
                ReplyAsync(ex.Message);
            }
        }
    }
}
