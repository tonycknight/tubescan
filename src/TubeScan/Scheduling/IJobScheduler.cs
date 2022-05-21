namespace TubeScan.Scheduling
{
    internal interface IJobScheduler
    {
        IJobScheduler Register(JobScheduleInfo info);

        void Start();

        void Stop();
    }
}
