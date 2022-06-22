using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Tk.Extensions;
using TubeScan.DiscordCommands;
using Xunit;

namespace TubeScan.Tests.DiscordCommands
{
    public class RenderingExtensionsTests
    {
        [Fact]
        public void RenderStationStatus_NullCrowding_Rendered()
        {
            var naptanId = "naptanId";
            var name = "name";

            var station = new Models.Station(naptanId, name);
            var status = new Models.StationStatus(naptanId);

            var result = station.RenderStationStatus(status, x => x, new Models.LineStatus[0], x => x);

            result.Should().NotBeNull();
            result.Title.Should().Be(station.ShortName);
            result.Description.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void RenderStationStatus_NullLiveCrowding_Rendered()
        {
            var naptanId = "naptanId";
            var name = "name";

            var station = new Models.Station(naptanId, name);
            var status = new Models.StationStatus(naptanId)
            {
                Crowding = new Models.StationCrowding()
                {
                    LivePercentageOfBaseline = null,
                    AveragePercentageOfBaseline = 0.1,
                }
            };

            var result = station.RenderStationStatus(status, x => x, new Models.LineStatus[0], x => x);

            result.Should().NotBeNull();
            result.Title.Should().Be(station.ShortName);
            result.Description.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void RenderStationStatus_NullAverageCrowding_Rendered()
        {
            var naptanId = "naptanId";
            var name = "name";

            var station = new Models.Station(naptanId, name);
            var status = new Models.StationStatus(naptanId)
            {
                Crowding = new Models.StationCrowding()
                {
                    LivePercentageOfBaseline = null,
                    AveragePercentageOfBaseline = 0.1,
                }
            };

            var result = station.RenderStationStatus(status, x => x, new Models.LineStatus[0], x => x);

            result.Should().NotBeNull();
            result.Title.Should().Be(station.ShortName);
            result.Description.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void RenderStationStatus_NonNullCrowding_Rendered()
        {
            var naptanId = "naptanId";
            var name = "name";

            var station = new Models.Station(naptanId, name);
            var status = new Models.StationStatus(naptanId)
            {
                Crowding = new Models.StationCrowding()
                {
                    LivePercentageOfBaseline = 0.2,
                    AveragePercentageOfBaseline = 0.1,
                }
            };

            var result = station.RenderStationStatus(status, x => x, new Models.LineStatus[0], x => x);

            result.Should().NotBeNull();
            result.Title.Should().Be(station.ShortName);
            result.Description.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void RenderStationStatus_NoArrivals_PartiallyRendered()
        {
            var naptanId = "naptanId";
            var name = "name";
            var lineId = "line1";
            var station = new Models.Station(naptanId, name, new[] { new Models.StationLine(lineId, lineId) });
            var status = new Models.StationStatus(naptanId);
            var lineStatuses = new[] { new Models.LineStatus() { Id = lineId, } };

            var result = station.RenderStationStatus(status, x => x, lineStatuses, x => x);

            result.Should().NotBeNull();
            result.Title.Should().Be(station.ShortName);
            result.Description.Should().NotBeNullOrEmpty();

            result.Description.Should().Contain("No arrivals");
        }

        [Fact]
        public void RenderStationStatus_NoArrivals_NoLines_PartiallyRendered()
        {
            var naptanId = "naptanId";
            var name = "name";
            var lineId = "line1";
            var station = new Models.Station(naptanId, name, new Models.StationLine[0]);
            var status = new Models.StationStatus(naptanId);
            var lineStatuses = new[] { new Models.LineStatus() { Id = lineId, } };

            var result = station.RenderStationStatus(status, x => x, lineStatuses, x => x);

            result.Should().NotBeNull();
            result.Title.Should().Be(station.ShortName);
            result.Description.Should().NotBeNullOrEmpty();

            result.Description.Should().Contain("No arrivals");
        }

        [Fact]
        public void RenderStationStatus_Arrivals_PartiallyRendered()
        {
            var naptanId = "naptanId";
            var name = "name";
            var lineId = "lineId";
            var lineName = "line name";
            var stationName = "station name";

            var arrivals = Enumerable.Range(1, 3)
                .Select(i => new Models.Arrival()
                {
                    CurrentLocation = $"loc {i}",
                    LineId = lineId,
                    DestinationId = i.ToString(),
                    VehicleId = i.ToString()
                })
                .ToArray();
                

            var station = new Models.Station(naptanId, name, new[] { new Models.StationLine(lineId, lineId) });
            var status = new Models.StationStatus(naptanId)
            {                
                Arrivals = arrivals
            };
            var lineStatuses = new[] { new Models.LineStatus() { Id = lineId, } };

            var result = station.RenderStationStatus(status, x => lineName, lineStatuses, x => stationName);

            var expectedLocations = arrivals.Select(a => a.CurrentLocation);

            result.Should().NotBeNull();
            result.Title.Should().Be(station.ShortName);
            result.Description.Should().NotBeNullOrEmpty();

            // without tying the tests too hard to the output, verify key information appears in the description
            expectedLocations.All(l =>
            {
                result.Description.Should().Contain(l);
                return true;
            });
            result.Description.Should().Contain(lineName);
            result.Description.Should().Contain(stationName);
        }


        [Theory]
        [InlineData(1)]
        [InlineData(3)]
        [InlineData(11)]
        public void RenderStationTags_Rendered(int count)
        {
            var naptans = Enumerable.Range(1, count).Select(x => $"naptan{x}").ToList();
            var stations = naptans.Select((n,x) => new Models.Station(n, $"station{x}")).ToList();
            var tags = naptans.Select((n, x) => new Models.StationTag(n, $"tag{x}")).ToList();

            var result = tags.RenderStationTags(stations);
            result.Should().NotBeNullOrWhiteSpace();

            tags.All(t => { result.Should().Contain(t.Tag); return true; });
            stations.All(s => { result.Should().Contain(s.ShortName); return true; });
            naptans.All(n => { result.Should().NotContain(n); return true; });
        }

        [Fact]
        public void RenderStationTags_UnknownStation_Rendered()
        {
            var naptan = "1234";
            var station = new Models.Station("abc", "a station");
            var tag = new Models.StationTag(naptan, "zzz");

            var tags = new[] { tag };
            var stations = new[] { station };

            var result = tags.RenderStationTags(stations);
            result.Should().NotBeNullOrWhiteSpace();

            stations.All(s => { result.Should().NotContain(s.ShortName); return true; });
            result.Should().Contain("Unknown");
        }


        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(3)]
        public void RenderLinesStatus_NoStatus_ContainsLineNames(int count)
        {
            var lines = Enumerable.Range(1, count).Select(i => new Models.Line(i.ToString(), $"Line{i}", "")).ToList();
            var status = new Dictionary<string, Models.LineStatus>();

            var result = lines.RenderLinesStatus(status, true);

            if (count == 0)
            {
                result.Should().BeEmpty();
            }
            else
            {
                var expected = lines.Select(l => $"**{l.Name}**: Unknown").Join(Environment.NewLine);
                result.Should().Be(expected);                
            }
        }

        

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(3)]
        public void RenderLinesStatus_WithStatus_ContainsLineNames(int count)
        {
            var tflHealth = "tflHealth2";
            var lines = Enumerable.Range(1, count).Select(i => new Models.Line(i.ToString(), $"Line{i}", "")).ToList();

            var status = lines.ToDictionary(l => l.Id, l => new Models.LineStatus()
            {
                Id = l.Id,
                HealthStatuses = new[] { new Models.LineHealthStatus() { TflHealth = tflHealth } },
            });

            var result = lines.RenderLinesStatus(status, true);

            if (count == 0)
            {
                result.Should().BeEmpty();
            }
            else
            {
                var expected = lines.Select(l => $"**{l.Name}**: :warning: ***{tflHealth}***").Join(Environment.NewLine);
                result.Should().Be(expected);
            }
        }


        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(3)]
        public void RenderLinesStatus_WithStatusDescription_ContainsLineNames(int count)
        {
            var tflHealth = "tflHealth2";
            var descr = "line description";
            var lines = Enumerable.Range(1, count).Select(i => new Models.Line(i.ToString(), $"Line{i}", "")).ToList();

            var status = lines.ToDictionary(l => l.Id, l => new Models.LineStatus()
            {
                Id = l.Id,
                HealthStatuses = new[] {
                    new Models.LineHealthStatus() {
                        TflHealth = tflHealth,
                        Description = descr,
                    }
                },
            });

            var result = lines.RenderLinesStatus(status, true);

            if (count == 0)
            {
                result.Should().BeEmpty();
            }
            else
            {                
                var freqs = result.Split(descr);
                freqs.Length.Should().Be(count + 1);

                freqs = result.Split(tflHealth);
                freqs.Length.Should().Be(count + 1);
            }
        }

        [Theory]
        [InlineData(1, 2)]
        [InlineData(1, 3)]
        [InlineData(3, 2)]
        [InlineData(3, 3)]
        [InlineData(3, 4)]
        public void RenderLinesStatus_WithManyStatus_ContainsLineNames(int lineCount, int statusCount)
        {            
            var lines = Enumerable.Range(1, lineCount)
                                    .Select(i => new Models.Line(i.ToString(), $"Line{i}", ""))
                                    .ToList();

            var status = lines.ToDictionary(l => l.Id, l => new Models.LineStatus()
            {
                Id = l.Id,
                HealthStatuses = Enumerable.Range(1, statusCount)
                                            .Select(j => new Models.LineHealthStatus() { TflHealth = $"tflHealth {Guid.NewGuid()}" })
                                            .ToList(),
            });

            var result = lines.RenderLinesStatus(status, true);

            if (lineCount == 0)
            {
                result.Should().BeEmpty();
            }
            else
            {
                lines.ForEach(l => result.Should().Contain(l.Name));
                var healthDescrs = status.Values.SelectMany(ls => ls.HealthStatuses).Select(lhs => lhs.TflHealth).ToList();
                healthDescrs.ForEach(x => result.Should().Contain(x));
            }
        }


        [Theory]
        [InlineData(1, 2)]
        [InlineData(1, 3)]
        [InlineData(3, 2)]
        [InlineData(3, 3)]
        [InlineData(3, 4)]
        public void RenderLinesStatus_WithAffectedStations_ContainsLineNames(int lineCount, int stationCount)
        {
            var station = "station";
            var lines = Enumerable.Range(1, lineCount)
                                    .Select(i => new Models.Line(i.ToString(), $"Line{i}", ""))
                                    .ToList();

            var status = lines.ToDictionary(l => l.Id, l => new Models.LineStatus()
            {
                Id = l.Id,
                HealthStatuses = new[] {
                                            new Models.LineHealthStatus()
                                            {
                                                TflHealth = "",
                                                AffectedStations = Enumerable.Range(1, stationCount)
                                                            .Select(k => new Models.Station("", $"{station} {Guid.NewGuid()}"))
                                                            .ToList()
                                            }
                                        },
            });

            var result = lines.RenderLinesStatus(status, true);

            if (lineCount == 0)
            {
                result.Should().BeEmpty();
            }
            else
            {
                var stationNames = status.SelectMany(ls => ls.Value.HealthStatuses)
                                            .SelectMany(hs => hs.AffectedStations)
                                            .Select(s => s.Name)
                                            .ToList();

                lines.ForEach(l => result.Should().Contain(l.Name));
                stationNames.ForEach(l => result.Should().Contain(l));

            }
        }
    }
}
