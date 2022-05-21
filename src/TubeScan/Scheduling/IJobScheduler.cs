namespace TubeScan.Scheduling
{
    internal interface IJobScheduler
    {
        IJobScheduler Register(IEnumerable<JobScheduleInfo> infos);

        void Start();

        void Stop();
    }
}
