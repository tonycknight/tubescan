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

            var js = new JobScheduler(t);
            js.Register(jobs);
            

            var check = js.Jobs.ToList();
            check.Should().BeEquivalentTo(jobs);
        }

        [Fact]
        public async Task Start_JobExecuted_AtLeastOnce()
        {
            var frequency = TimeSpan.FromSeconds(1);
            var testDuration = TimeSpan.FromSeconds(10);            
            var t = CreateMockTelemetry();

            using var js = new JobScheduler(t);

            var job = Substitute.For<IJob>();
            var jobInfo = new JobScheduleInfo(job, frequency);
                        
            js.Register(jobInfo.Singleton());

                        
            js.Start();

            await Task.Delay(testDuration);

            js.Stop();
            
            int expectedInvocations = (int)(testDuration.TotalMilliseconds / frequency.TotalMilliseconds); // round down

            job.Received(expectedInvocations);
        }

        private ITelemetry CreateMockTelemetry() => Substitute.For<ITelemetry>();
    }
}
