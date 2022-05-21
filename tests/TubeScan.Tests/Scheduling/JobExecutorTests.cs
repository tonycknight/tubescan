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
            var now = DateTime.UtcNow;
            var tp = CreateMockTimeProvider();
            tp.UtcNow().Returns(now);
            var t = CreateMockTelemetry();

            var job = Substitute.For<IJob>();            
            var jobInfo = new JobScheduleInfo(job, TimeSpan.Zero);

            var je = new JobExecutor(tp, t);

            var result = await je.ExecuteJobAsync(jobInfo);

            result.Should().BeOfType<JobExecuteResultOk>();
            result.Duration.Should().BeGreaterThan(TimeSpan.Zero);
            await job.Received(1).ExecuteAsync();
            t.Received(2).Message(Arg.Any<string>());
            t.Received(0).Error(Arg.Any<string>());
            jobInfo.LastExecution.Should().Be(now);
        }

        [Fact]
        public async Task ExecuteJobAsync_JobExecuted_ThrowsException()
        {
            var now = DateTime.UtcNow;
            var tp = CreateMockTimeProvider();
            tp.UtcNow().Returns(now);
            var t = CreateMockTelemetry();
                       

            var job = Substitute.For<IJob>();
            job.When(j => j.ExecuteAsync()).Do(ci => throw new Exception());
            var jobInfo = new JobScheduleInfo(job, TimeSpan.Zero);


            var je = new JobExecutor(tp, t);

            var result = await je.ExecuteJobAsync(jobInfo) as JobExecuteResultError;

            result.Should().NotBeNull();

            result.Exception.Should().NotBeNull();
            result.Duration.Should().BeGreaterThan(TimeSpan.Zero);
            await job.Received(1).ExecuteAsync();
            t.Received(1).Message(Arg.Any<string>());
            t.Received(1).Error(Arg.Any<string>());
            jobInfo.LastExecution.Should().Be(now);
        }

        [Fact]
        public async Task ExecuteJobAsync_ZeroTime_JobNotExecuted_ReturnsNotRun()
        {
            var tp = CreateMockTimeProvider();
            var t = CreateMockTelemetry();

            var job = Substitute.For<IJob>();
            var jobInfo = new JobScheduleInfo(job, TimeSpan.FromDays(1));

            var je = new JobExecutor(tp, t);

            var result = await je.ExecuteJobAsync(jobInfo);

            result.Should().BeOfType<JobExecuteResultNotRun>();
            result.Duration.Should().Be(TimeSpan.Zero);
            await job.Received(0).ExecuteAsync();
            t.Received(0).Message(Arg.Any<string>());
            t.Received(0).Error(Arg.Any<string>());
            jobInfo.LastExecution.Should().Be(DateTime.MinValue);
        }

        [Fact]
        public async Task ExecuteJobAsync_LastExecutionTooSoon_JobNotExecuted_ReturnsNotRun()
        {
            var frequency = TimeSpan.FromDays(1);
            var now = DateTime.UtcNow;
            var tp = CreateMockTimeProvider();
            tp.UtcNow().Returns(now);
            var t = CreateMockTelemetry();
            

            var job = Substitute.For<IJob>();
            var jobInfo = new JobScheduleInfo(job, frequency);
            jobInfo.LastExecution = now - TimeSpan.FromSeconds(1);
            var je = new JobExecutor(tp, t);

            var result = await je.ExecuteJobAsync(jobInfo);

            result.Should().BeOfType<JobExecuteResultNotRun>();
            result.Duration.Should().Be(TimeSpan.Zero);
            await job.Received(0).ExecuteAsync();
            t.Received(0).Message(Arg.Any<string>());
            t.Received(0).Error(Arg.Any<string>());
            jobInfo.LastExecution.Should().Be(now - TimeSpan.FromSeconds(1));
        }

        [Fact]
        public async Task ExecuteJobAsync_LastExecutionTooOld_JobExecuted_ReturnsOk()
        {
            var frequency = TimeSpan.FromDays(1);
            var now = DateTime.UtcNow;
            var tp = CreateMockTimeProvider();
            tp.UtcNow().Returns(now);
            var t = CreateMockTelemetry();


            var job = Substitute.For<IJob>();
            var jobInfo = new JobScheduleInfo(job, frequency);
            jobInfo.LastExecution = now - frequency;
            var je = new JobExecutor(tp, t);

            var result = await je.ExecuteJobAsync(jobInfo);

            result.Should().BeOfType<JobExecuteResultOk>();
            result.Duration.Should().BeGreaterThan(TimeSpan.Zero);
            await job.Received(1).ExecuteAsync();
            t.Received(2).Message(Arg.Any<string>());
            t.Received(0).Error(Arg.Any<string>());
            jobInfo.LastExecution.Should().Be(now);
        }

        private ITimeProvider CreateMockTimeProvider() => Substitute.For<ITimeProvider>();

        private ITelemetry CreateMockTelemetry() => Substitute.For<ITelemetry>();
    }

#pragma warning restore CS8602 // Dereference of a possibly null reference.
}
