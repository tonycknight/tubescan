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
        }
    }
}
