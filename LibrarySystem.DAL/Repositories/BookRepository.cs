using LibrarySystem.Core.Entities;
using LibrarySystem.Core.Interfaces;
using LibrarySystem.DAL.Connection;
using Microsoft.Data.SqlClient;

namespace LibrarySystem.DAL.Repositories
{
    public class BookRepository : IBookRepository
    {
        private readonly ISqlConnectionFactory _connectionFactory;

        public BookRepository(ISqlConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<IEnumerable<Book>> SearchBooksAsync(string searchTerm)
        {
            var books = new List<Book>();
            using var connection = (SqlConnection)_connectionFactory.CreateConnection();

            string query = @"
                SELECT BookId, Title, Author, ISBN, IsAvailable, CreatedAt 
                FROM Books 
                WHERE (Title LIKE @SearchTerm 
                   OR Author LIKE @SearchTerm 
                   OR ISBN = @ExactMatch) 
                  AND IsDeleted = 0";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@SearchTerm", $"%{searchTerm}%");
            command.Parameters.AddWithValue("@ExactMatch", searchTerm);

            try
            {
                await connection.OpenAsync();
                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    books.Add(new Book
                    {
                        BookId = reader.GetInt32(reader.GetOrdinal("BookId")),
                        Title = reader.GetString(reader.GetOrdinal("Title")),
                        Author = reader.GetString(reader.GetOrdinal("Author")),
                        ISBN = reader.GetString(reader.GetOrdinal("ISBN")),
                        IsAvailable = reader.GetBoolean(reader.GetOrdinal("IsAvailable")),
                        CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
                    });
                }
            }
            catch (SqlException ex)
            {
                throw new Exception("A database error occurred while searching for books.", ex);
            }
            return books;
        }

        public async Task<bool> UpdateAvailabilityAsync(int bookId, bool isAvailable)
        {
            using var connection = (SqlConnection)_connectionFactory.CreateConnection();
            string query = "UPDATE Books SET IsAvailable = @IsAvailable, UpdatedAt = GETDATE() WHERE BookId = @BookId AND IsDeleted = 0";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@IsAvailable", isAvailable);
            command.Parameters.AddWithValue("@BookId", bookId);

            await connection.OpenAsync();
            return await command.ExecuteNonQueryAsync() > 0;
        }

        public async Task<Book?> GetByIdAsync(int bookId)
        {
            using var connection = (SqlConnection)_connectionFactory.CreateConnection();
            string query = "SELECT * FROM Books WHERE BookId = @BookId AND IsDeleted = 0";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@BookId", bookId);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new Book
                {
                    BookId = reader.GetInt32(reader.GetOrdinal("BookId")),
                    Title = reader.GetString(reader.GetOrdinal("Title")),
                    Author = reader.GetString(reader.GetOrdinal("Author")),
                    ISBN = reader.GetString(reader.GetOrdinal("ISBN")),
                    IsAvailable = reader.GetBoolean(reader.GetOrdinal("IsAvailable")),
                    CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
                };
            }
            return null;
        }

        public async Task<IEnumerable<Book>> GetAllBooksAsync()
        {
            var books = new List<Book>();
            using var connection = (SqlConnection)_connectionFactory.CreateConnection();
            string query = "SELECT * FROM Books WHERE IsDeleted = 0 ORDER BY CreatedAt DESC";

            using var command = new SqlCommand(query, connection);
            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                books.Add(new Book
                {
                    BookId = reader.GetInt32(reader.GetOrdinal("BookId")),
                    Title = reader.GetString(reader.GetOrdinal("Title")),
                    Author = reader.GetString(reader.GetOrdinal("Author")),
                    ISBN = reader.GetString(reader.GetOrdinal("ISBN")),
                    IsAvailable = reader.GetBoolean(reader.GetOrdinal("IsAvailable")),
                    CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
                });
            }
            return books;
        }

        public async Task<bool> AddBookAsync(Book book)
        {
            using var connection = (SqlConnection)_connectionFactory.CreateConnection();
            string query = @"
                INSERT INTO Books (Title, Author, ISBN, IsAvailable, CreatedAt, CreatedBy, IsDeleted) 
                VALUES (@Title, @Author, @ISBN, @IsAvailable, GETDATE(), @CreatedBy, 0)";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Title", book.Title);
            command.Parameters.AddWithValue("@Author", book.Author);
            command.Parameters.AddWithValue("@ISBN", book.ISBN);
            command.Parameters.AddWithValue("@IsAvailable", book.IsAvailable);
            command.Parameters.AddWithValue("@CreatedBy", (object?)book.CreatedBy ?? DBNull.Value);

            try
            {
                await connection.OpenAsync();
                return await command.ExecuteNonQueryAsync() > 0;
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2627) throw new Exception("Failed to add book. ISBN must be unique.");
                throw;
            }
        }

        public async Task<bool> UpdateBookAsync(Book book)
        {
            using var connection = (SqlConnection)_connectionFactory.CreateConnection();
            string query = @"
                UPDATE Books 
                SET Title = @Title, Author = @Author, ISBN = @ISBN, IsAvailable = @IsAvailable, 
                    UpdatedAt = GETDATE(), UpdatedBy = @UpdatedBy 
                WHERE BookId = @BookId AND IsDeleted = 0";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Title", book.Title);
            command.Parameters.AddWithValue("@Author", book.Author);
            command.Parameters.AddWithValue("@ISBN", book.ISBN);
            command.Parameters.AddWithValue("@IsAvailable", book.IsAvailable);
            command.Parameters.AddWithValue("@UpdatedBy", (object?)book.UpdatedBy ?? DBNull.Value);
            command.Parameters.AddWithValue("@BookId", book.BookId);

            await connection.OpenAsync();
            return await command.ExecuteNonQueryAsync() > 0;
        }
        public async Task<bool> DeleteBookAsync(int bookId, int deletedByUserId)
        {
            using var connection = (SqlConnection)_connectionFactory.CreateConnection();
            string query = @"
                UPDATE Books 
                SET IsDeleted = 1, DeletedAt = GETDATE(), DeletedBy = @DeletedBy 
                WHERE BookId = @BookId AND IsDeleted = 0";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@BookId", bookId);
            command.Parameters.AddWithValue("@DeletedBy", deletedByUserId);

            try
            {
                await connection.OpenAsync();
                return await command.ExecuteNonQueryAsync() > 0;
            }
            catch (SqlException ex)
            {
                if (ex.Number == 547) return false;
                throw;
            }
        }

        public async Task<bool> IsIsbnExistsAsync(string isbn, int excludeBookId = 0)
        {
            using var connection = (SqlConnection)_connectionFactory.CreateConnection();

            string query = "SELECT COUNT(1) FROM Books WHERE ISBN = @ISBN AND BookId != @ExcludeBookId AND IsDeleted = 0";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@ISBN", isbn);
            command.Parameters.AddWithValue("@ExcludeBookId", excludeBookId);

            await connection.OpenAsync();
            return Convert.ToInt32(await command.ExecuteScalarAsync()) > 0;
        }

        public async Task<bool> IsBookCurrentlyBorrowedAsync(int bookId)
        {
            using var connection = (SqlConnection)_connectionFactory.CreateConnection();
            string query = "SELECT COUNT(1) FROM Borrowings WHERE BookId = @BookId AND ReturnDate IS NULL";
            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@BookId", bookId);

            await connection.OpenAsync();
            return Convert.ToInt32(await command.ExecuteScalarAsync()) > 0;
        }
    }
}