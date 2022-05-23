using TubeScan.Models;

namespace TubeScan.Users
{
    internal interface IUsersRepository
    {
        Task<IList<User>> GetAllUsersAsync();

        Task SetUserAsync(User value);
    }
}
