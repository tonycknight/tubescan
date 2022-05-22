using System.Diagnostics.CodeAnalysis;
using TubeScan.Telemetry;

namespace TubeScan.Scheduling
{
    [ExcludeFromCodeCoverage]
    internal class JobScheduler : IJobScheduler, IDisposable
    {
        private readonly ITelemetry _telemetry;
        private readonly List<JobScheduleTimer> _jobs = new List<JobScheduleTimer>();

        public IEnumerable<JobScheduleInfo> Jobs => _jobs.Select(t => t.Info);

        public JobScheduler(ITelemetry telemetry)
        {
            _telemetry = telemetry;
        }

        ~JobScheduler()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        public IJobScheduler Register(IEnumerable<JobScheduleInfo> infos)
        {
            var timers = infos.Select(CreateJobScheduleTimer);

            _jobs.AddRange(timers);

            return this;
        }

        public void Start()
        {
            foreach (var job in _jobs)
            {
                job.Timer.Start();
            }
        }

        public void Stop()
        {
            foreach (var job in _jobs)
            {
                job.Timer.Stop();
            }
        }

        private void Dispose(bool disposing)
        {
            foreach (var job in _jobs)
            {
                job.Timer.Dispose();
            }
            _jobs.Clear();
        }

        private JobScheduleTimer CreateJobScheduleTimer(JobScheduleInfo info)
        {
            var timer = new System.Timers.Timer()
            {
                Interval = info.Frequency.TotalMilliseconds,
                AutoReset = true,
                Enabled = true,
            };

            timer.Elapsed += (object? sender, System.Timers.ElapsedEventArgs e) =>
            {
                timer.Stop();
                var r = ExecuteJob(info).GetAwaiter().GetResult();
                timer.Start();
            };

            return new JobScheduleTimer(timer, info);
        }

        private Task<JobExecuteResult> ExecuteJob(JobScheduleInfo job)
        {
            var executor = new JobExecutor(_telemetry);

            return executor.ExecuteJobAsync(job);
        }
    }
}
