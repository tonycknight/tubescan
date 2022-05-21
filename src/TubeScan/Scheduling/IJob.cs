namespace TubeScan.Scheduling
{
    internal interface IJob
    {
        Task ExecuteAsync();
    }
}
