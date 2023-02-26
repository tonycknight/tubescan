using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using Tk.Extensions.Tasks;
using Tk.Extensions.Time;
using TubeScan.Lines;
using TubeScan.Stations;
using TubeScan.Tfl;
using Xunit;

namespace TubeScan.Tests.Stations
{
    public class TflStationProviderTests
    {
        [Theory]
        [InlineData(3, 0)]
        public void GetStationsAsync_ErrorResponseFromTfl_ExceptionRaised(int lineCount, int stationCount)
        {
            IList<Models.Line>? lines = Enumerable.Range(1, lineCount).Select(i => new Models.Line(i.ToString(), "", "")).ToList();

            var lp = Substitute.For<ILineProvider>();
            lp.GetLinesAsync().Returns(lines.ToTaskResult());

            var tflc = Substitute.For<ITflClient>();
            var stations = Enumerable.Range(1, stationCount)
                                     .Select(i => new TflStation()
                                     {
                                         NaptanId = i.ToString(),
                                         CommonName = "test"
                                     })
                                     .ToArray();
            var expectedStations = stations.Select(tfls => new Models.Station(tfls.NaptanId, tfls.CommonName)).ToList();

            var tflr = new TflResponse()
            {
                Body = "",
                IsSuccess = false,
            };
            tflc.GetAsync(Arg.Any<string>(), Arg.Any<bool>()).Returns(tflr.ToTaskResult());

            var sp = new TflStationProvider(tflc, lp, GetTimeProvider(DateTime.UtcNow));

            var a = async () =>
            {
                await sp.GetStationsAsync();
                return true;
            };

            a.Should().ThrowAsync<ApplicationException>().WithMessage("?*");
        }


        [Theory]
        [InlineData(1, 1)]
        [InlineData(1, 3)]
        [InlineData(3, 1)]
        [InlineData(3, 5)]
        public async Task GetStationsAsync_StationsReturned(int lineCount, int stationCount)
        {
            IList<Models.Line>? lines = Enumerable.Range(1, lineCount).Select(i => new Models.Line(i.ToString(), "", "")).ToList();

            var lp = Substitute.For<ILineProvider>();
            lp.GetLinesAsync().Returns(lines.ToTaskResult());

            var tflc = Substitute.For<ITflClient>();
            var stations = Enumerable.Range(1, stationCount)
                                     .Select(i => new TflStation()
                                     {
                                         NaptanId = i.ToString(),
                                         CommonName = "test"
                                     })
                                     .ToArray();
            var expectedStations = stations.Select(tfls => new Models.Station(tfls.NaptanId, tfls.CommonName)).ToList();

            var tflr = new TflResponse()
            {
                Body = Newtonsoft.Json.JsonConvert.SerializeObject(stations),
                IsSuccess = true,
            };
            tflc.GetAsync(Arg.Is<string>(s => s.StartsWith("Line")), false).Returns(tflr.ToTaskResult());

            var sp = new TflStationProvider(tflc, lp, GetTimeProvider(DateTime.UtcNow));

            var result = await sp.GetStationsAsync();

            result.Should().BeEquivalentTo(expectedStations);
        }


        [Theory]
        [InlineData("", 0.1, 1.0)]
        [InlineData(" ", 0.2, 2.0)]
        [InlineData("abc", 0.3, 3.0)]
        public async Task GetStationStatusAsync_CrowdingMapped(string naptanId, double livePct, double avgPct)
        {
            var now = new DateTime(2022, 04, 01, 12, 0, 0, DateTimeKind.Utc);
            var stations = new[] { new TflStation()
                                     {
                                         NaptanId = naptanId,
                                         CommonName = "test"
                                     } };
            var liveCrowding = $"{{ dataAvailable: true, percentageOfBaseline: {livePct} }}";
            var crowding = new TflDayOfWeekStationCrowding()
            {
                NaptanId = naptanId,
                TimeBands = new[]
                {
                    new TflTimeBandStationCrowding()
                    {
                        PercentageOfBaseLine = 1000.0,
                        TimeBand = "11:00-12:00"
                    },
                    new TflTimeBandStationCrowding()
                    {
                        PercentageOfBaseLine = avgPct,
                        TimeBand = "12:00-13:00"
                    },
                    new TflTimeBandStationCrowding()
                    {
                        PercentageOfBaseLine = 1000.0,
                        TimeBand = "13:00-14:00"
                    },
                },
            };
            var lp = Substitute.For<ILineProvider>();
            var tflc = Substitute.For<ITflClient>();
            var tflr1 = new TflResponse()
            {
                Body = liveCrowding,
                IsSuccess = true,
            };
            var tflr2 = new TflResponse()
            {
                Body = Newtonsoft.Json.JsonConvert.SerializeObject(crowding),
                IsSuccess = true,
            };
            var tflr3 = new TflResponse()
            {
                Body = Newtonsoft.Json.JsonConvert.SerializeObject(stations),
                IsSuccess = true,
            };

            tflc.GetAsync($"crowding/{naptanId}/Live", true).Returns(tflr1.ToTaskResult());
            tflc.GetAsync(Arg.Is<string>(s => s.StartsWith($"crowding/{naptanId}/Fri")), true).Returns(tflr2.ToTaskResult());
            tflc.GetAsync(Arg.Is<string>(s => s.StartsWith($"StopPoint/{naptanId}/Arrivals")), true).Returns(tflr3.ToTaskResult());

            var sp = new TflStationProvider(tflc, lp, GetTimeProvider(now));

            var r = await sp.GetStationStatusAsync(naptanId);

            r.Should().NotBeNull();
            r.Crowding.LivePercentageOfBaseline.Should().Be(livePct);
            r.Crowding.AveragePercentageOfBaseline.Should().Be(avgPct);
        }

        [Theory]
        [InlineData("", 0.1, 0)]
        [InlineData(" ", 0.2, 1)]
        [InlineData("abc", 0.3, 3)]
        public async Task GetStationStatusAsync_NullOrOutOfRangeTimeBands_CrowdingMapped(string naptanId, double livePct, int timeBandCount)
        {
            var now = new DateTime(2022, 04, 01, 12, 0, 0, DateTimeKind.Utc);
            var stations = new[] { new TflStation()
                                     {
                                         NaptanId = naptanId,
                                         CommonName = "test"
                                     } };
            var liveCrowding = $"{{ dataAvailable: true, percentageOfBaseline: {livePct} }}";
            var crowding = new TflDayOfWeekStationCrowding()
            {
                NaptanId = naptanId,
                TimeBands = Enumerable.Range(1, timeBandCount)
                                      .Select(i => new TflTimeBandStationCrowding() { PercentageOfBaseLine = 1000, TimeBand = "00:00-00:01" })
                                      .ToList(),
            };
            var lp = Substitute.For<ILineProvider>();
            var tflc = Substitute.For<ITflClient>();
            var tflr1 = new TflResponse()
            {
                Body = liveCrowding,
                IsSuccess = true,
            };
            var tflr2 = new TflResponse()
            {
                Body = Newtonsoft.Json.JsonConvert.SerializeObject(crowding),
                IsSuccess = true,
            };
            var tflr3 = new TflResponse()
            {
                Body = Newtonsoft.Json.JsonConvert.SerializeObject(stations),
                IsSuccess = true,
            };

            tflc.GetAsync($"crowding/{naptanId}/Live", true).Returns(tflr1.ToTaskResult());
            tflc.GetAsync(Arg.Is<string>(s => s.StartsWith($"crowding/{naptanId}/Fri")), true).Returns(tflr2.ToTaskResult());
            tflc.GetAsync(Arg.Is<string>(s => s.StartsWith($"StopPoint/{naptanId}/Arrivals")), true).Returns(tflr3.ToTaskResult());

            var sp = new TflStationProvider(tflc, lp, GetTimeProvider(now));

            var r = await sp.GetStationStatusAsync(naptanId);

            r.Should().NotBeNull();
            r.Crowding.LivePercentageOfBaseline.Should().Be(livePct);
            r.Crowding.AveragePercentageOfBaseline.Should().BeNull();
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("abc")]

        public void GetStationStatusAsync_ErrorResponseFromLiveTfl_ExceptionThrown(string naptanId)
        {
            var now = new DateTime(2022, 04, 01, 12, 0, 0, DateTimeKind.Utc);
            var lp = Substitute.For<ILineProvider>();
            var tflc = Substitute.For<ITflClient>();
            var tflr1 = new TflResponse()
            {
                Body = "",
                IsSuccess = false,
            };
            var tflr2 = new TflResponse()
            {
                Body = "{}",
                IsSuccess = true,
            };
            var tflr3 = new TflResponse()
            {
                Body = "[]",
                IsSuccess = true,
            };

            tflc.GetAsync($"crowding/{naptanId}/Live", true).Returns(tflr1.ToTaskResult());
            tflc.GetAsync(Arg.Is<string>(s => s.StartsWith($"crowding/{naptanId}/Fri")), true).Returns(tflr2.ToTaskResult());
            tflc.GetAsync(Arg.Is<string>(s => s.StartsWith($"StopPoint/{naptanId}/Arrivals")), true).Returns(tflr3.ToTaskResult());

            var sp = new TflStationProvider(tflc, lp, GetTimeProvider(now));

            var a = async () =>
            {
                await sp.GetStationStatusAsync(naptanId);
                return true;
            };

            a.Should().ThrowAsync<ApplicationException>().WithMessage("?*");
        }


        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("abc")]

        public void GetStationStatusAsync_ErrorResponseFromAverageTfl_ExceptionThrown(string naptanId)
        {
            var liveCrowding = "{ dataAvailable: true, percentageOfBaseline: 1.0 }";
            var now = new DateTime(2022, 04, 01, 12, 0, 0, DateTimeKind.Utc);
            var lp = Substitute.For<ILineProvider>();
            var tflc = Substitute.For<ITflClient>();
            var tflr1 = new TflResponse()
            {
                Body = liveCrowding,
                IsSuccess = true,
            };
            var tflr2 = new TflResponse()
            {
                Body = "{}",
                IsSuccess = false,
            };
            var tflr3 = new TflResponse()
            {
                Body = "[]",
                IsSuccess = true,
            };

            tflc.GetAsync($"crowding/{naptanId}/Live", true).Returns(tflr1.ToTaskResult());
            tflc.GetAsync(Arg.Is<string>(s => s.StartsWith($"crowding/{naptanId}/Fri")), true).Returns(tflr2.ToTaskResult());
            tflc.GetAsync(Arg.Is<string>(s => s.StartsWith($"StopPoint/{naptanId}/Arrivals")), true).Returns(tflr3.ToTaskResult());

            var sp = new TflStationProvider(tflc, lp, GetTimeProvider(now));

            var a = async () =>
            {
                await sp.GetStationStatusAsync(naptanId);
                return true;
            };


            a.Should().ThrowAsync<ApplicationException>().WithMessage("?*");
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("abc")]

        public void GetStationStatusAsync_ErrorResponseFromArrivalsTfl_ExceptionThrown(string naptanId)
        {
            var liveCrowding = "{ dataAvailable: true, percentageOfBaseline: 1.0 }";
            var now = new DateTime(2022, 04, 01, 12, 0, 0, DateTimeKind.Utc);
            var lp = Substitute.For<ILineProvider>();
            var tflc = Substitute.For<ITflClient>();
            var tflr1 = new TflResponse()
            {
                Body = liveCrowding,
                IsSuccess = true,
            };
            var tflr2 = new TflResponse()
            {
                Body = "{}",
                IsSuccess = true,
            };
            var tflr3 = new TflResponse()
            {
                Body = "[]",
                IsSuccess = false,
            };

            tflc.GetAsync($"crowding/{naptanId}/Live", true).Returns(tflr1.ToTaskResult());
            tflc.GetAsync(Arg.Is<string>(s => s.StartsWith($"crowding/{naptanId}/Fri")), true).Returns(tflr2.ToTaskResult());
            tflc.GetAsync(Arg.Is<string>(s => s.StartsWith($"StopPoint/{naptanId}/Arrivals")), true).Returns(tflr3.ToTaskResult());

            var sp = new TflStationProvider(tflc, lp, GetTimeProvider(now));

            var a = async () =>
            {
                await sp.GetStationStatusAsync(naptanId);
                return true;
            };


            a.Should().ThrowAsync<ApplicationException>().WithMessage("?*");
        }

        private ITimeProvider GetTimeProvider(DateTime now)
        {
            var r = Substitute.For<ITimeProvider>();
            r.UtcNow().Returns(now);
            return r;
        }
    }
}
