namespace TubeScan
{
    internal interface ISettable<T>
    {
        void Set(IList<T> values);
    }
}
