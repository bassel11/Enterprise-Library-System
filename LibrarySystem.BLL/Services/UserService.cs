using LibrarySystem.BLL.Interfaces;
using LibrarySystem.Core.Entities;
using LibrarySystem.Core.Interfaces;

namespace LibrarySystem.BLL.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<(bool Success, string Message)> RegisterUserAsync(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                return (false, "Username and password are required.");

            var existingUser = await _userRepository.GetByUsernameAsync(username.Trim());
            if (existingUser != null)
                return (false, "Username already exists. Please choose another.");

            var newUser = new User
            {
                Username = username.Trim(),
                Role = "User"
            };

            newUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);

            bool isCreated = await _userRepository.CreateUserAsync(newUser);
            return isCreated ? (true, "Registration successful!") : (false, "Error occurred during registration.");
        }

        public async Task<User?> AuthenticateUserAsync(string username, string password)
        {
            var user = await _userRepository.GetByUsernameAsync(username.Trim());

            if (user == null) return null;

            if (!user.IsActive)
                throw new UnauthorizedAccessException("Your account has been deactivated by the Administrator.");

            bool isValidPassword = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
            if (isValidPassword)
            {
                return user;
            }

            return null;
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync() => await _userRepository.GetAllUsersAsync();
        public async Task<bool> ToggleUserStatusAsync(int userId, bool isActive, int updatedByUserId)
            => await _userRepository.ToggleUserStatusAsync(userId, isActive, updatedByUserId);
    }
}