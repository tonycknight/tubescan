namespace TubeScan.Models
{
    internal class User
    {
        public User(ulong id, string mention)
        {
            Id = id;
            Mention = mention;
        }

        public ulong Id { get; }
        public string Mention { get; }
        public ulong UserId { get; }
    }
}
