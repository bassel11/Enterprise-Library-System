using LibrarySystem.DAL.Connection;
using Microsoft.Data.SqlClient;

namespace LibrarySystem.DAL.Seeding
{
    public static class DatabaseSeeder
    {
        public static async Task SeedAdminUserAsync(ISqlConnectionFactory connectionFactory)
        {
            using var connection = (SqlConnection)connectionFactory.CreateConnection();

            string checkQuery = "SELECT COUNT(1) FROM Users WHERE Role = 'Admin'";
            using var checkCommand = new SqlCommand(checkQuery, connection);

            try
            {
                await connection.OpenAsync();

                int adminCount = Convert.ToInt32(await checkCommand.ExecuteScalarAsync());

                if (adminCount == 0)
                {
                    string insertQuery = @"
                        INSERT INTO Users (Username, PasswordHash, Role, IsActive, CreatedAt, IsDeleted) 
                        VALUES (@Username, @PasswordHash, 'Admin', 1, GETDATE(), 0)";

                    using var insertCommand = new SqlCommand(insertQuery, connection);
                    insertCommand.Parameters.AddWithValue("@Username", "Admin");

                    string hashedPassword = BCrypt.Net.BCrypt.HashPassword("Admin123");
                    insertCommand.Parameters.AddWithValue("@PasswordHash", hashedPassword);

                    await insertCommand.ExecuteNonQueryAsync();
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Database seeding failed: {ex.Message}");
            }
        }
    }
}