using LibrarySystem.Core.Entities;

namespace LibrarySystem.BLL.Interfaces
{
    public interface IBookService
    {
        Task<IEnumerable<Book>> SearchBooksAsync(string searchTerm);
        Task<IEnumerable<Book>> GetAllBooksAsync();
        Task<bool> AddBookAsync(Book book);
        Task<bool> UpdateBookAsync(Book book);
        Task<bool> DeleteBookAsync(int bookId, int deletedByUserId);
    }
}
