using Tk.Extensions;
using TubeScan.Models;

namespace TubeScan.Lines
{
    internal static class LineStatusExtensions
    {
        public static IList<LineStatus> ToLineStatuses(this string json) 
            => Newtonsoft.Json.JsonConvert.DeserializeObject<TflLineStatusCheck[]>(json)
                                          .Select(ToLineStatus)
                                          .ToList();

        public static LineStatus ToLineStatus(TflLineStatusCheck dto)
        {
            var activeStatuses = dto.LineStatuses.NormaliseToActiveStatuses().ToList();

            var lineHealth = activeStatuses.Select(ToLineHealthStatus)
                                           .GroupBy(lhs => lhs.Description)
                                           .SelectMany(grp => grp.OrderByDescending(lhs => lhs.AffectedStations.Count).Take(1));

            return new LineStatus()
            {
                Id = dto.Id,
                HealthStatuses = lineHealth.ToList(),
            };
        }

        private static IEnumerable<TflLineStatus> NormaliseToActiveStatuses(this IEnumerable<TflLineStatus> values)
            => values.NullToEmpty()
                .Select(tls =>
                {
                    tls.ValidityPeriods = tls.ValidityPeriods.Where(vp => !vp.IsNow.HasValue ||
                                                                           vp.IsNow.Value).ToArray();
                    return tls;
                });

        private static LineHealthStatus ToLineHealthStatus(this TflLineStatus value)
            => new LineHealthStatus()
            {
                AffectedStations = (value.Disruption?.AffectedStops ?? new TflAffectedStop[0])
                                                .DistinctBy(tas => tas.NaptanId)
                                                .Select(tas => new Station(tas.NaptanId, tas.Name))
                                                .OrderBy(s => s.Name)
                                                .ToList(),
                Description = value.Reason,
                TflHealth = value.StatusSeverityDescription,
                Health = ToLineHealthStatus(value.StatusSeverityDescription)
            };

        private static HealthStatus ToLineHealthStatus(string description) 
            => description switch
            {
                "Good Service" => HealthStatus.Ok,
                "Severe Delays" => HealthStatus.SevereDelays,
                "Minor Delays" => HealthStatus.PartialService,
                "Part Suspended" => HealthStatus.PartialService,
                "Part Closure" => HealthStatus.PartialService,
                "Special Service" => HealthStatus.PartialService,
                "Service Closed" => HealthStatus.NoService,
                _ => HealthStatus.Unknown
            };
    }
}
