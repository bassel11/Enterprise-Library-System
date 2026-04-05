using LibrarySystem.BLL.Interfaces;
using LibrarySystem.Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LibrarySystem.UI.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IBookService _bookService;
        private readonly IUserService _userService;

        public AdminController(IBookService bookService, IUserService userService)
        {
            _bookService = bookService;
            _userService = userService;
        }

        private int CurrentAdminId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var allBooks = await _bookService.GetAllBooksAsync();
            var allUsers = await _userService.GetAllUsersAsync();

            ViewBag.TotalBooks = allBooks.Count();
            ViewBag.AvailableBooks = allBooks.Count(b => b.IsAvailable);
            ViewBag.BorrowedBooks = allBooks.Count(b => !b.IsAvailable);

            ViewBag.TotalUsers = allUsers.Count();
            ViewBag.ActiveUsers = allUsers.Count(u => u.IsActive);

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ManageBooks()
        {
            var books = await _bookService.GetAllBooksAsync();
            return View(books);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddBook(Book book)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    book.IsAvailable = true;
                    book.CreatedBy = CurrentAdminId;

                    bool isAdded = await _bookService.AddBookAsync(book);
                    if (isAdded)
                    {
                        TempData["SuccessMessage"] = "Book added successfully!";
                        return RedirectToAction(nameof(ManageBooks));
                    }
                }
                catch (InvalidOperationException ex)
                {
                    TempData["ErrorMessage"] = ex.Message;
                    return RedirectToAction(nameof(ManageBooks));
                }
                catch (Exception)
                {
                    TempData["ErrorMessage"] = "An unexpected error occurred while adding the book.";
                    return RedirectToAction(nameof(ManageBooks));
                }
            }

            TempData["ErrorMessage"] = "Failed to add the book. Please check your inputs.";
            return RedirectToAction(nameof(ManageBooks));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditBook(Book book)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    book.UpdatedBy = CurrentAdminId;

                    bool isUpdated = await _bookService.UpdateBookAsync(book);
                    if (isUpdated)
                    {
                        TempData["SuccessMessage"] = "Book updated successfully!";
                        return RedirectToAction(nameof(ManageBooks));
                    }
                }
                catch (InvalidOperationException ex)
                {
                    TempData["ErrorMessage"] = ex.Message;
                    return RedirectToAction(nameof(ManageBooks));
                }
                catch (Exception)
                {
                    TempData["ErrorMessage"] = "An unexpected error occurred while updating the book.";
                    return RedirectToAction(nameof(ManageBooks));
                }
            }

            TempData["ErrorMessage"] = "Failed to update the book. Please ensure all fields are correctly filled.";
            return RedirectToAction(nameof(ManageBooks));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteBook(int bookId)
        {
            bool isDeleted = await _bookService.DeleteBookAsync(bookId, CurrentAdminId);
            if (isDeleted)
            {
                TempData["SuccessMessage"] = "Book deleted successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "Cannot delete this book. It might be currently borrowed.";
            }
            return RedirectToAction(nameof(ManageBooks));
        }

        [HttpGet]
        public async Task<IActionResult> ManageUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return View(users);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleUserStatus(int userId, bool isActive)
        {
            bool success = await _userService.ToggleUserStatusAsync(userId, isActive, CurrentAdminId);
            if (success)
                TempData["SuccessMessage"] = isActive ? "User activated successfully." : "User deactivated successfully.";
            else
                TempData["ErrorMessage"] = "Failed to change user status.";

            return RedirectToAction(nameof(ManageUsers));
        }
    }
}