using LibrarySystem.Core.Entities;

namespace LibrarySystem.Core.Interfaces
{
    public interface IUserRepository
    {
        Task<bool> CreateUserAsync(User user);
        Task<User?> GetByUsernameAsync(string username);
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<bool> ToggleUserStatusAsync(int userId, bool isActive, int updatedByUserId);
    }
}
