using TubeScan.Models;

namespace TubeScan.Users
{
    internal static class DtoExtensions
    {
        public static User FromDto(this UserDto dto)
            => new User(dto.UserId);
    }
}
