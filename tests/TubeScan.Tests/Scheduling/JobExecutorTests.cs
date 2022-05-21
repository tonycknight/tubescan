using System;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using Tk.Extensions.Time;
using TubeScan.Scheduling;
using TubeScan.Telemetry;
using Xunit;

namespace TubeScan.Tests.Scheduling
{
    public class JobExecutorTests
    {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
        [Fact]
        public async Task ExecuteJobAsync_JobExecuted_ReturnsOk()
        {
            var t = CreateMockTelemetry();

            var job = Substitute.For<IJob>();            
            var jobInfo = new JobScheduleInfo(job, TimeSpan.Zero);

            var je = new JobExecutor(t);

            var result = await je.ExecuteJobAsync(jobInfo);

            result.Should().BeOfType<JobExecuteResultOk>();
            result.Duration.Should().BeGreaterThan(TimeSpan.Zero);
            await job.Received(1).ExecuteAsync();
            t.Received(2).Message(Arg.Any<string>());
            t.Received(0).Error(Arg.Any<string>());
        }

        [Fact]
        public async Task ExecuteJobAsync_JobExecuted_ThrowsException_ReturnsError()
        {
            var t = CreateMockTelemetry();

            var job = Substitute.For<IJob>();
            job.When(j => j.ExecuteAsync()).Do(ci => throw new Exception());
            var jobInfo = new JobScheduleInfo(job, TimeSpan.Zero);


            var je = new JobExecutor(t);

            var result = await je.ExecuteJobAsync(jobInfo) as JobExecuteResultError;

            result.Should().NotBeNull();

            result.Exception.Should().NotBeNull();
            result.Duration.Should().BeGreaterThan(TimeSpan.Zero);
            await job.Received(1).ExecuteAsync();
            t.Received(1).Message(Arg.Any<string>());
            t.Received(1).Error(Arg.Any<string>());
        }

        private ITelemetry CreateMockTelemetry() => Substitute.For<ITelemetry>();
    }

#pragma warning restore CS8602 // Dereference of a possibly null reference.
}
