using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using Tk.Extensions;
using TubeScan.Scheduling;
using TubeScan.Telemetry;
using Xunit;

namespace TubeScan.Tests.Scheduling
{
    public class JobSchedulerTests
    {
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(3)] 
        public void Jobs_Matches_RegisteredJobs(int count)
        {
            var t = CreateMockTelemetry();

            var jobs = Enumerable.Range(1, count)
                                 .Select(_ => new JobScheduleInfo(Substitute.For<IJob>(), TimeSpan.FromSeconds(1)))
                                 .ToList();

            using var js = new JobScheduler(t);
            js.Register(jobs);
            

            var check = js.Jobs.ToList();
            check.Should().BeEquivalentTo(jobs);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(3)]
        public void Dispose_RepeatedDispose_NoExceptionsThrown(int count)
        {
            var t = CreateMockTelemetry();

            var jobs = Enumerable.Range(1, count)
                                 .Select(_ => new JobScheduleInfo(Substitute.For<IJob>(), TimeSpan.FromSeconds(1)))
                                 .ToList();

            var js = new JobScheduler(t);
            js.Register(jobs);

            js.Dispose();
            js.Dispose();
        }

        private ITelemetry CreateMockTelemetry() => Substitute.For<ITelemetry>();
    }
}
