namespace TubeScan.Scheduling
{
    internal abstract class JobExecuteResult
    {
        protected JobExecuteResult(IJob job, TimeSpan duration)
        {
            Job = job;
            Duration = duration;
        }

        public IJob Job { get; }
        public TimeSpan Duration { get; }                
    }

    internal class JobExecuteResultOk : JobExecuteResult
    {
        public JobExecuteResultOk(IJob job, TimeSpan duration) : base(job, duration)
        {
        }
    }

    internal class JobExecuteResultError : JobExecuteResult
    {
        public JobExecuteResultError(IJob job, TimeSpan duration, Exception exception) : base(job, duration)
        {
            Exception = exception;
        }

        public Exception Exception { get; }
    }
}
