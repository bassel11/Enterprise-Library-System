using LibrarySystem.Core.DTOs;

namespace LibrarySystem.BLL.Interfaces
{
    public interface IBorrowingService
    {
        Task<bool> BorrowBookAsync(int userId, int bookId);
        Task<bool> ReturnBookAsync(int borrowId, int bookId);
        Task<IEnumerable<UserBorrowingDto>> GetUserBorrowingsAsync(int userId);
    }
}
