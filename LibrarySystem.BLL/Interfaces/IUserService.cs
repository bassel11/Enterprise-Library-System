using LibrarySystem.Core.Entities;

namespace LibrarySystem.BLL.Interfaces
{
    public interface IUserService
    {
        Task<(bool Success, string Message)> RegisterUserAsync(string username, string password);
        Task<User?> AuthenticateUserAsync(string username, string password);
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<bool> ToggleUserStatusAsync(int userId, bool isActive, int updatedByUserId);
    }
}
