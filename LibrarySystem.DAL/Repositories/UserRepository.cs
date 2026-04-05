using LibrarySystem.Core.Entities;
using LibrarySystem.Core.Interfaces;
using LibrarySystem.DAL.Connection;
using Microsoft.Data.SqlClient;

namespace LibrarySystem.DAL.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ISqlConnectionFactory _connectionFactory;

        public UserRepository(ISqlConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<bool> CreateUserAsync(User user)
        {
            using var connection = (SqlConnection)_connectionFactory.CreateConnection();
            string query = @"
                INSERT INTO Users (Username, PasswordHash, Role, IsActive, CreatedAt, IsDeleted) 
                VALUES (@Username, @PasswordHash, @Role, 1, GETDATE(), 0)";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Username", user.Username);
            command.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
            command.Parameters.AddWithValue("@Role", user.Role);

            try
            {
                await connection.OpenAsync();
                return await command.ExecuteNonQueryAsync() > 0;
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2627) return false;
                throw new Exception("Database error while creating user.", ex);
            }
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            using var connection = (SqlConnection)_connectionFactory.CreateConnection();

            string query = "SELECT * FROM Users WHERE Username = @Username AND IsDeleted = 0";
            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Username", username);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new User
                {
                    UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                    Username = reader.GetString(reader.GetOrdinal("Username")),
                    PasswordHash = reader.GetString(reader.GetOrdinal("PasswordHash")),
                    Role = reader.GetString(reader.GetOrdinal("Role")),
                    IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                    CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
                };
            }
            return null;
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            var users = new List<User>();
            using var connection = (SqlConnection)_connectionFactory.CreateConnection();
            string query = "SELECT * FROM Users WHERE Role = 'User' AND IsDeleted = 0 ORDER BY CreatedAt DESC";
            using var command = new SqlCommand(query, connection);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                users.Add(new User
                {
                    UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                    Username = reader.GetString(reader.GetOrdinal("Username")),
                    Role = reader.GetString(reader.GetOrdinal("Role")),
                    IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                    CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
                });
            }
            return users;
        }

        public async Task<bool> ToggleUserStatusAsync(int userId, bool isActive, int updatedByUserId)
        {
            using var connection = (SqlConnection)_connectionFactory.CreateConnection();
            string query = @"
                UPDATE Users 
                SET IsActive = @IsActive, UpdatedAt = GETDATE(), UpdatedBy = @UpdatedBy 
                WHERE UserId = @UserId AND Role = 'User' AND IsDeleted = 0";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@IsActive", isActive);
            command.Parameters.AddWithValue("@UserId", userId);
            command.Parameters.AddWithValue("@UpdatedBy", updatedByUserId);

            await connection.OpenAsync();
            return await command.ExecuteNonQueryAsync() > 0;
        }
    }
}