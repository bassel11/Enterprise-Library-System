using LibrarySystem.BLL.Interfaces;
using LibrarySystem.Core.DTOs;
using LibrarySystem.Core.Interfaces;
using System.Transactions;

namespace LibrarySystem.BLL.Services
{
    public class BorrowingService : IBorrowingService
    {
        private readonly IBookRepository _bookRepository;
        private readonly IBorrowingRepository _borrowingRepository;

        public BorrowingService(IBookRepository bookRepository, IBorrowingRepository borrowingRepository)
        {
            _bookRepository = bookRepository;
            _borrowingRepository = borrowingRepository;
        }

        public async Task<bool> BorrowBookAsync(int userId, int bookId)
        {
            if (userId <= 0 || bookId <= 0)
                throw new ArgumentException("Invalid User or Book ID.");

            var book = await _bookRepository.GetByIdAsync(bookId);
            if (book == null)
                throw new Exception("Book not found.");

            if (!book.IsAvailable)
                throw new InvalidOperationException($"The book '{book.Title}' is currently checked out and not available.");

            DateTime dueDate = DateTime.Now.AddDays(14);

            return await _borrowingRepository.BorrowBookAsync(userId, bookId, dueDate);
        }

        public async Task<bool> ReturnBookAsync(int borrowId, int bookId)
        {
            using (var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                bool isReturned = await _borrowingRepository.MarkAsReturnedAsync(borrowId);
                bool isUpdated = await _bookRepository.UpdateAvailabilityAsync(bookId, true);

                if (isReturned && isUpdated)
                {
                    transaction.Complete();
                    return true;
                }

                return false;
            }
        }

        public async Task<IEnumerable<UserBorrowingDto>> GetUserBorrowingsAsync(int userId)
        {
            return await _borrowingRepository.GetUserBorrowingsAsync(userId);
        }
    }
}