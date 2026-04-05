using LibrarySystem.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibrarySystem.UI.Controllers
{
    [Authorize(Roles = "User")]
    public class UserController : Controller
    {
        private readonly IBookService _bookService;
        private readonly IBorrowingService _borrowingService;

        public UserController(IBookService bookService, IBorrowingService borrowingService)
        {
            _bookService = bookService;
            _borrowingService = borrowingService;
        }

        [HttpGet]
        public IActionResult Dashboard()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Search(string? query)
        {
            ViewBag.SearchQuery = query;

            if (string.IsNullOrWhiteSpace(query))
            {
                return View(Enumerable.Empty<LibrarySystem.Core.Entities.Book>());
            }

            var results = await _bookService.SearchBooksAsync(query);
            return View(results);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BorrowBook(int bookId, string? searchQuery)
        {
            try
            {
                int userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");

                if (userId <= 0)
                {
                    TempData["ErrorMessage"] = "Session expired or invalid user. Please login again.";
                    return RedirectToAction("Login", "Auth");
                }

                bool success = await _borrowingService.BorrowBookAsync(userId, bookId);

                if (success)
                    TempData["SuccessMessage"] = "Book borrowed successfully! Enjoy reading.";
                else
                    TempData["ErrorMessage"] = "Failed to borrow the book. It might be already checked out.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction(nameof(Search), new { query = searchQuery });
        }

        [HttpGet]
        public async Task<IActionResult> MyBorrowings()
        {
            int userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");

            if (userId <= 0)
            {
                return RedirectToAction("Login", "Auth");
            }

            var myBorrowings = await _borrowingService.GetUserBorrowingsAsync(userId);
            return View(myBorrowings);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReturnBook(int borrowId, int bookId)
        {
            try
            {
                bool success = await _borrowingService.ReturnBookAsync(borrowId, bookId);

                if (success)
                    TempData["SuccessMessage"] = "Book returned successfully. Thank you!";
                else
                    TempData["ErrorMessage"] = "Failed to return the book.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction(nameof(MyBorrowings));
        }
    }
}
