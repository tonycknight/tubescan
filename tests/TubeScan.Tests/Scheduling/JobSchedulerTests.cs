using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using Tk.Extensions.Time;
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
        public void Register_MatchesJobs(int count)
        {
            var cycle = TimeSpan.FromSeconds(1);
            var testDuration = TimeSpan.FromSeconds(10);
            var tp = CreateMockTimeProvider();
            var t = CreateMockTelemetry();

            var js = new JobScheduler(tp, t);

            var jobs = Enumerable.Range(1, count).Select(_ =>
            {
                var job = Substitute.For<IJob>();
                return new JobScheduleInfo(job, cycle);
            }).ToList();

            foreach (var job in jobs)
            {
                js.Register(job);                
            }

            var check = js.Jobs.ToList();
            check.Should().BeEquivalentTo(jobs);
        }


        [Theory]
        [InlineData(0, false)]
        [InlineData(1, true)]
        public void Start_IsStarted_Matches(int count, bool expected)
        {
            var cycle = TimeSpan.FromSeconds(1);
            var tp = CreateMockTimeProvider();
            var t = CreateMockTelemetry();

            var js = new JobScheduler(tp, t);
            js.Started.Should().BeFalse();

            var jobs = Enumerable.Range(1, count)
                                 .Select(_ => new JobScheduleInfo(Substitute.For<IJob>(), cycle))
                                 .ToList();

            foreach (var job in jobs)
            {
                js.Register(job);
            }

            js.Start();
            js.Started.Should().Be(expected);

        }

        [Fact(Skip = "Temporary for GH")]
        public async Task Start_NoJobs_NoJobExecuted()
        {            
            var testDuration = TimeSpan.FromSeconds(10);
            var tp = CreateMockTimeProvider();
            var t = CreateMockTelemetry();

            var js = new JobScheduler(tp, t);

            var sw = Stopwatch.StartNew();

            js.Start();

            await Task.Delay(testDuration);

            js.Stop();

            sw.Stop();

        }

        [Fact(Skip = "Temporary for GH")]
        public async Task Start_JobExecuted_AtLeastOnce()
        {
            var cycle = TimeSpan.FromSeconds(1);
            var testDuration = TimeSpan.FromSeconds(10);
            var tp = CreateMockTimeProvider();
            var t = CreateMockTelemetry();

            var js = new JobScheduler(tp, t);

            var job = Substitute.For<IJob>();
            var jobInfo = new JobScheduleInfo(job, cycle);
                        
            js.Register(jobInfo);

            var sw = Stopwatch.StartNew();
            
            js.Start();

            await Task.Delay(testDuration);

            js.Stop();

            sw.Stop();

            int expectedInvocations = (int)(testDuration.TotalMilliseconds / cycle.TotalMilliseconds); // round down

            job.Received(expectedInvocations);
        }


        private ITimeProvider CreateMockTimeProvider() => Substitute.For<ITimeProvider>();

        private ITelemetry CreateMockTelemetry() => Substitute.For<ITelemetry>();
    }
}
