using TubeScan.Models;

namespace TubeScan.Users
{
    internal interface IUsersRepository
    {
        Task<IList<User>> GetAllUserIdsAsync();

        Task SetUserAsync(User value);
    }
}
