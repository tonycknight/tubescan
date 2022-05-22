namespace TubeScan.Scheduling
{
    internal interface IJobScheduler
    {
        IEnumerable<JobScheduleInfo> Jobs { get; }
        IJobScheduler Register(IEnumerable<JobScheduleInfo> infos);

        void Start();

        void Stop();
    }
}
