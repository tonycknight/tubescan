using Tk.Extensions.Time;
using TubeScan.Telemetry;

namespace TubeScan.Scheduling
{
    internal class JobScheduler : IJobScheduler, IDisposable
    {
        private readonly IList<JobScheduleInfo> _jobs = new List<JobScheduleInfo>();
        private readonly System.Timers.Timer _timer;
        private readonly ITimeProvider _time;
        private readonly ITelemetry _telemetry;
        private readonly TimeSpan _cycleTime;

        public JobScheduler(ITimeProvider time, ITelemetry telemetry)
        {
            _time = time;
            _telemetry = telemetry;
            _cycleTime = TimeSpan.FromSeconds(1);
            _timer = new System.Timers.Timer();
        }

        ~JobScheduler() => Dispose(false);

        public IEnumerable<JobScheduleInfo> Jobs => _jobs;

        public bool Started => _timer.Enabled;

        public IJobScheduler Register(IEnumerable<JobScheduleInfo> infos)
        {
            foreach (var info in infos)
            {
                _jobs.Add(info);
            }

            return this;
        }

        public void Start()
        {
            if (_jobs.Count >= 1)
            {                
                _timer.Interval = _cycleTime.TotalMilliseconds;
                _timer.Elapsed += timer_Elapsed;
                _timer.AutoReset = true;
                _timer.Enabled = true;
                _timer.Start();
            }
        }


        public void Stop()
        {
            _timer.Stop();
            _timer.Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            Stop();
        }

        private void timer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            CycleJobs();
        }

        private void CycleJobs()
        {
            _telemetry.Message($"Starting job cycle...");

            var tasks = _jobs.Select(ExecuteJob).ToArray();

            var results = Task.WhenAll(tasks);

            _telemetry.Message($"Finished job cycle.");
        }

        private Task<JobExecuteResult> ExecuteJob(JobScheduleInfo job)
        {
            var executor = new JobExecutor(_time, _telemetry);

            return executor.ExecuteJobAsync(job);
        }
    }
}
