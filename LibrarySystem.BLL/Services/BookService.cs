using LibrarySystem.BLL.Interfaces;
using LibrarySystem.Core.Entities;
using LibrarySystem.Core.Interfaces;

namespace LibrarySystem.BLL.Services
{
    public class BookService : IBookService
    {
        private readonly IBookRepository _bookRepository;

        public BookService(IBookRepository bookRepository)
        {
            _bookRepository = bookRepository;
        }

        public async Task<IEnumerable<Book>> SearchBooksAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                throw new ArgumentException("Search term cannot be empty.");
            if (searchTerm.Length > 100)
                throw new ArgumentException("Search term is too long.");

            return await _bookRepository.SearchBooksAsync(searchTerm.Trim());
        }

        public async Task<IEnumerable<Book>> GetAllBooksAsync()
        {
            return await _bookRepository.GetAllBooksAsync();
        }

        public async Task<bool> AddBookAsync(Book book)
        {
            if (string.IsNullOrWhiteSpace(book.Title) || string.IsNullOrWhiteSpace(book.Author) || string.IsNullOrWhiteSpace(book.ISBN))
                throw new ArgumentException("Title, Author, and ISBN are required.");

            if (await _bookRepository.IsIsbnExistsAsync(book.ISBN))
                throw new InvalidOperationException($"The ISBN '{book.ISBN}' is already registered.");

            return await _bookRepository.AddBookAsync(book);
        }

        public async Task<bool> UpdateBookAsync(Book book)
        {
            if (book.BookId <= 0) throw new ArgumentException("Invalid Book ID.");

            if (await _bookRepository.IsIsbnExistsAsync(book.ISBN, book.BookId))
                throw new InvalidOperationException($"The ISBN '{book.ISBN}' is already in use by another book.");

            var existingBook = await _bookRepository.GetByIdAsync(book.BookId);
            if (existingBook == null) throw new Exception("Book not found.");

            if (!existingBook.IsAvailable && book.IsAvailable)
            {
                bool isActuallyBorrowed = await _bookRepository.IsBookCurrentlyBorrowedAsync(book.BookId);
                if (isActuallyBorrowed)
                {
                    throw new InvalidOperationException("Cannot manually make this book available because it is currently checked out by a user.");
                }
            }

            return await _bookRepository.UpdateBookAsync(book);
        }

        public async Task<bool> DeleteBookAsync(int bookId, int deletedByUserId)
        {
            if (bookId <= 0)
                throw new ArgumentException("Invalid Book ID.");

            return await _bookRepository.DeleteBookAsync(bookId, deletedByUserId);
        }
    }
}