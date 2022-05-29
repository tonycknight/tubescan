using Discord;
using Tk.Extensions;
using Tk.Extensions.Time;

namespace TubeScan.DiscordCommands
{
    internal static class RenderingExtensions
    {
        public static string RenderStationTags(this IEnumerable<Models.StationTag> tags, IEnumerable<Models.Station> stations)
        {
            var stationsLookup = stations.ToDictionary(s => s.NaptanId, s => s, StringComparer.InvariantCultureIgnoreCase);

            return tags.Select(st =>
            {
                var station = stationsLookup.GetOrDefault(st.NaptanId)?.ShortName ?? "Unknown";

                return $"Tag **``{st.Tag}``** = ``{station}``";

            }).Join(Environment.NewLine);
        }

        public static Embed RenderStationStatus(this Models.Station station, Models.StationStatus status, Func<string, string> lineName, Func<string, string> stationName)
        {
            var crowdingLines = status.GetCrowdingLines();
            var arrivalsLines = status.GetArrivalsLines(lineName, stationName);

            var lines = crowdingLines.Concat(arrivalsLines).Join(Environment.NewLine);

            return new EmbedBuilder()
                        .WithTitle(station.ShortName)
                        .WithDescription(lines)
                        .Build();
        }

        public static string RenderLinesStatus(this IEnumerable<Models.Line> lines, Dictionary<string, Models.LineStatus> stats, bool fullDetails)
            => lines.Where(l => l != null)
                    .OrderBy(l => l.Name)
                    .SelectMany(line =>
                    {
                        var stat = stats.GetOrDefault(line.Id);

                        return GetLineStatus(line, stat, fullDetails);
                    })
                    .Join(Environment.NewLine);

        private static IEnumerable<string> GetCrowdingLines(this Models.StationStatus status)
        {
            var livePct = status.Crowding?.LivePercentageOfBaseline * 100;
            var avgPct = status.Crowding?.AveragePercentageOfBaseline * 100;

            Func<double?, string> fmt = v => v.HasValue ? $"{v:F2}%" : "Unknown";

            var lines = new[]
            {
                $"**Current Crowding**: {fmt(livePct)}",
                $"**Average Crowding**: {fmt(avgPct)}",
            };

            return lines;
        }

        private static IEnumerable<string> GetArrivalsLines(this Models.StationStatus status, 
                                                            Func<string, string> lineName, Func<string, string> stationName)
        {
            var lineGroups = status.Arrivals.NullToEmpty()
                                   .GroupBy(a => a.LineId);
            if (!lineGroups.Any())
            {
                yield return "_**No arrivals**_";
            }

            foreach(var lineGroup in lineGroups)
            {
                yield return $"**{lineName(lineGroup.Key) ?? lineGroup.Key}**";

                var trains = lineGroup.Where(t => !string.IsNullOrEmpty(t.VehicleId))
                    .GroupBy(t => t.DestinationId)
                    .Select(grp => grp.OrderBy(t => t.ExpectedArrival).First())
                    .OrderBy(t => t.ExpectedArrival);
                                
                foreach(var train in trains.Take(5))
                {
                    var dest = stationName(train.DestinationId) ?? "Unknown";

                    yield return $"To **{dest}** arriving at **{train.ExpectedArrival.DateTime.ToUkDateTime().ToString("HH:mm:ss")}**";
                    
                    if (!string.IsNullOrEmpty(train.CurrentLocation))
                    {
                        yield return $"> *{train.CurrentLocation}*";
                    }
                }
            }
        }


        private static IEnumerable<string> GetLineStatus(Models.Line line, Models.LineStatus status, bool fullDetails)
        {
            var header = $"**{line.Name}**";
            if (status == null)
            {
                return GetLineStatus(header, fullDetails);
            }
            return GetLineStatus(header, status, fullDetails);
        }

        private static IEnumerable<string> GetLineStatus(string header, bool fullDetails)
        {
            if (fullDetails)
            {
                yield return $"{header}: Unknown";
            }
            else
            {
                yield break;
            }
        }

        private static IEnumerable<string> GetLineStatus(string header, Models.LineStatus status, bool fullDetails)
        {            
            if (status.HealthStatuses.Count > 1)
            {
                yield return header;
            }
            foreach (var s in status.HealthStatuses)
            {
                yield return status.HealthStatuses.Count > 1 ? GetLineStatus(s) : $"{header}: {GetLineStatus(s)}";
                if (!string.IsNullOrEmpty(s.Description))
                {
                    yield return $"> {s.Description}";
                }
                if (fullDetails && s.AffectedStations?.Any() == true)
                {
                    yield return $"> **Affected stations**: {s.AffectedStations.Select(s => $"``{s.ShortName}``").Join(", ")}";
                }
            }
        }

        private static string GetLineStatus(this Models.LineHealthStatus status)
        {            
            
            var formattedText = status.Health == Models.HealthStatus.GoodService 
                                    ? status.TflHealth 
                                    : $"***{status.TflHealth}***";

            var emoji = status.Health switch
            {
                Models.HealthStatus.GoodService => "",
                Models.HealthStatus.MinorDelays => ":warning:",
                Models.HealthStatus.SevereDelays => ":fire:",
                Models.HealthStatus.PartialService => ":boom:",
                Models.HealthStatus.NoService => ":no_entry:",
                Models.HealthStatus.Unknown => ":warning:",
                _ => ":warning:",
            };

            return $"{emoji} {formattedText}";
        }
    }
}
