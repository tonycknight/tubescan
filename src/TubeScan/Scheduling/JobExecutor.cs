using System.Diagnostics;
using Tk.Extensions.Time;
using TubeScan.Telemetry;

namespace TubeScan.Scheduling
{
    internal class JobExecutor
    {
        private readonly ITimeProvider _time;
        private readonly ITelemetry _telemetry;

        public JobExecutor(ITimeProvider time, ITelemetry telemetry)
        {
            _time = time;
            _telemetry = telemetry;
        }

        public async Task<JobExecuteResult> ExecuteJobAsync(JobScheduleInfo job)
        {
            var sw = new Stopwatch();
            var now = _time.UtcNow();
            var nextRunTime = now - job.LastExecution;

            if (nextRunTime >= job.Frequency)
            {
                try
                {
                    sw.Start();
                    _telemetry.Message($"Starting job {job.Job.GetType().Name}...");
                    await job.Job.ExecuteAsync();
                    _telemetry.Message($"Finished job {job.Job.GetType().Name}.");
                    sw.Stop();

                    return new JobExecuteResultOk(job.Job, sw.Elapsed);
                }
                catch (Exception ex)
                {
                    sw.Stop();
                    _telemetry.Error(ex.Message);
                    return new JobExecuteResultError(job.Job, sw.Elapsed, ex);
                }
                finally
                {
                    job.LastExecution = _time.UtcNow();
                }
            }

            return new JobExecuteResultNotRun(job.Job);
        }
    }
}
