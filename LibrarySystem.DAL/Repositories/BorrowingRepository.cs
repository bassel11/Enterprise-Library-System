using LibrarySystem.Core.DTOs;
using LibrarySystem.Core.Interfaces;
using LibrarySystem.DAL.Connection;
using Microsoft.Data.SqlClient;

namespace LibrarySystem.DAL.Repositories
{
    public class BorrowingRepository : IBorrowingRepository
    {
        private readonly ISqlConnectionFactory _connectionFactory;

        public BorrowingRepository(ISqlConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<bool> BorrowBookAsync(int userId, int bookId, DateTime dueDate)
        {
            using var connection = (SqlConnection)_connectionFactory.CreateConnection();
            await connection.OpenAsync();

            using var transaction = connection.BeginTransaction();

            try
            {
                string updateQuery = @"
                    UPDATE Books 
                    SET IsAvailable = 0, UpdatedAt = GETDATE(), UpdatedBy = @UserId 
                    WHERE BookId = @BookId AND IsAvailable = 1 AND IsDeleted = 0";

                using var updateCmd = new SqlCommand(updateQuery, connection, transaction);
                updateCmd.Parameters.AddWithValue("@BookId", bookId);
                updateCmd.Parameters.AddWithValue("@UserId", userId);

                int rowsAffected = await updateCmd.ExecuteNonQueryAsync();

                if (rowsAffected == 0)
                {
                    transaction.Rollback();
                    throw new InvalidOperationException("Concurrency Conflict: This book was just borrowed by another user.");
                }

                string insertQuery = @"
                    INSERT INTO Borrowings (UserId, BookId, BorrowDate, DueDate) 
                    VALUES (@UserId, @BookId, GETDATE(), @DueDate)";

                using var insertCmd = new SqlCommand(insertQuery, connection, transaction);
                insertCmd.Parameters.AddWithValue("@UserId", userId);
                insertCmd.Parameters.AddWithValue("@BookId", bookId);
                insertCmd.Parameters.AddWithValue("@DueDate", dueDate);

                await insertCmd.ExecuteNonQueryAsync();

                transaction.Commit();
                return true;
            }
            catch (SqlException ex)
            {
                transaction.Rollback();
                throw new Exception("Database error occurred while processing the borrowing transaction.", ex);
            }
        }

        public async Task<bool> MarkAsReturnedAsync(int borrowId)
        {
            using var connection = (SqlConnection)_connectionFactory.CreateConnection();
            string query = "UPDATE Borrowings SET ReturnDate = GETDATE() WHERE BorrowId = @BorrowId AND ReturnDate IS NULL";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@BorrowId", borrowId);

            try
            {
                await connection.OpenAsync();
                int rowsAffected = await command.ExecuteNonQueryAsync();
                return rowsAffected > 0;
            }
            catch (SqlException ex)
            {
                throw new Exception("Database error occurred while marking the book as returned.", ex);
            }
        }

        public async Task<IEnumerable<UserBorrowingDto>> GetUserBorrowingsAsync(int userId)
        {
            var borrowings = new List<UserBorrowingDto>();
            using var connection = (SqlConnection)_connectionFactory.CreateConnection();

            string query = @"
                SELECT br.BorrowId, br.BookId, b.Title, b.Author, b.ISBN, br.BorrowDate, br.DueDate, br.ReturnDate 
                FROM Borrowings br
                INNER JOIN Books b ON br.BookId = b.BookId
                WHERE br.UserId = @UserId
                ORDER BY br.BorrowDate DESC";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@UserId", userId);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                borrowings.Add(new UserBorrowingDto
                {
                    BorrowId = reader.GetInt32(reader.GetOrdinal("BorrowId")),
                    BookId = reader.GetInt32(reader.GetOrdinal("BookId")),
                    BookTitle = reader.GetString(reader.GetOrdinal("Title")),
                    Author = reader.GetString(reader.GetOrdinal("Author")),
                    ISBN = reader.GetString(reader.GetOrdinal("ISBN")),
                    BorrowDate = reader.GetDateTime(reader.GetOrdinal("BorrowDate")),
                    DueDate = reader.GetDateTime(reader.GetOrdinal("DueDate")),
                    ReturnDate = reader.IsDBNull(reader.GetOrdinal("ReturnDate"))
                                 ? (DateTime?)null
                                 : reader.GetDateTime(reader.GetOrdinal("ReturnDate"))
                });
            }
            return borrowings;
        }
    }
}