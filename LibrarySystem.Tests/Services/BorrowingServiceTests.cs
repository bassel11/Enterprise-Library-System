using LibrarySystem.BLL.Services;
using LibrarySystem.Core.Entities;
using LibrarySystem.Core.Interfaces;
using Moq;

namespace LibrarySystem.Tests.Services
{
    public class BorrowingServiceTests
    {
        private readonly Mock<IBookRepository> _mockBookRepo;
        private readonly Mock<IBorrowingRepository> _mockBorrowingRepo;
        private readonly BorrowingService _borrowingService;

        public BorrowingServiceTests()
        {
            _mockBookRepo = new Mock<IBookRepository>();
            _mockBorrowingRepo = new Mock<IBorrowingRepository>();

            _borrowingService = new BorrowingService(_mockBookRepo.Object, _mockBorrowingRepo.Object);
        }

        [Fact]
        public async Task BorrowBookAsync_ShouldThrowException_WhenBookIsNotAvailable()
        {
            int userId = 1;
            int bookId = 100;

            var unavailableBook = new Book
            {
                BookId = bookId,
                Title = "Clean Architecture",
                IsAvailable = false
            };

            _mockBookRepo.Setup(repo => repo.GetByIdAsync(bookId))
                         .ReturnsAsync(unavailableBook);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _borrowingService.BorrowBookAsync(userId, bookId));

            Assert.Contains("not available", exception.Message);

            _mockBorrowingRepo.Verify(repo => repo.BorrowBookAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<DateTime>()), Times.Never);
        }

        [Fact]
        public async Task BorrowBookAsync_ShouldReturnTrue_WhenBookIsAvailable()
        {
            int userId = 1;
            int bookId = 100;

            var availableBook = new Book
            {
                BookId = bookId,
                Title = "Design Patterns",
                IsAvailable = true
            };

            _mockBookRepo.Setup(repo => repo.GetByIdAsync(bookId))
                         .ReturnsAsync(availableBook);

            _mockBorrowingRepo.Setup(repo => repo.BorrowBookAsync(userId, bookId, It.IsAny<DateTime>()))
                              .ReturnsAsync(true);

            bool result = await _borrowingService.BorrowBookAsync(userId, bookId);

            Assert.True(result);

            _mockBorrowingRepo.Verify(repo => repo.BorrowBookAsync(userId, bookId, It.IsAny<DateTime>()), Times.Once);
        }
    }
}