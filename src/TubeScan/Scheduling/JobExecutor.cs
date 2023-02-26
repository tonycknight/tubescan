using System.Diagnostics;
using Tk.Extensions.Time;
using TubeScan.Telemetry;

namespace TubeScan.Scheduling
{
    internal class JobExecutor
    {
        private readonly ITelemetry _telemetry;

        public JobExecutor(ITelemetry telemetry)
        {
            _telemetry = telemetry;
        }

        public async Task<JobExecuteResult> ExecuteJobAsync(JobScheduleInfo job)
        {
            var sw = new Stopwatch();

            try
            {
                sw.Start();
                $"Starting job {job.Job.GetType().Name}...".CreateTelemetryEvent().Send(_telemetry);
                await job.Job.ExecuteAsync();
                $"Finished job {job.Job.GetType().Name}.".CreateTelemetryEvent().Send(_telemetry);
                sw.Stop();

                return new JobExecuteResultOk(job.Job, sw.Elapsed);
            }
            catch (Exception ex)
            {
                sw.Stop();
                ex.Message.CreateTelemetryEvent(TelemetryEventKind.Error).Send(_telemetry);
                return new JobExecuteResultError(job.Job, sw.Elapsed, ex);
            }
        }
    }
}
