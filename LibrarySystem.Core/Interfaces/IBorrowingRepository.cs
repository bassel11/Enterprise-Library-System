using LibrarySystem.Core.DTOs;

namespace LibrarySystem.Core.Interfaces
{
    public interface IBorrowingRepository
    {
        Task<bool> BorrowBookAsync(int userId, int bookId, DateTime dueDate);

        Task<bool> MarkAsReturnedAsync(int borrowId);
        Task<IEnumerable<UserBorrowingDto>> GetUserBorrowingsAsync(int userId);
    }
}