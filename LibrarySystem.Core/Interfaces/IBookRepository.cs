using LibrarySystem.Core.Entities;

namespace LibrarySystem.Core.Interfaces
{
    public interface IBookRepository
    {
        Task<IEnumerable<Book>> SearchBooksAsync(string searchTerm);
        Task<IEnumerable<Book>> GetAllBooksAsync();
        Task<Book?> GetByIdAsync(int bookId);
        Task<bool> AddBookAsync(Book book);
        Task<bool> UpdateBookAsync(Book book);
        Task<bool> UpdateAvailabilityAsync(int bookId, bool isAvailable);
        Task<bool> DeleteBookAsync(int bookId, int deletedByUserId);

        Task<bool> IsIsbnExistsAsync(string isbn, int excludeBookId = 0);
        Task<bool> IsBookCurrentlyBorrowedAsync(int bookId);
    }
}
