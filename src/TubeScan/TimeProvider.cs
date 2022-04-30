namespace TubeScan
{
    public interface ITimeProvider
    {
        DateTime UtcNow();
    }

    internal class TimeProvider : ITimeProvider
    {
        public DateTime UtcNow() => DateTime.UtcNow;
    }
}
