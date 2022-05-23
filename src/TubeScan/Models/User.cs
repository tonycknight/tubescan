namespace TubeScan.Models
{
    internal class User
    {
        public User(ulong id)
        {
            Id = id;
        }

        public ulong Id { get; }
    }
}
