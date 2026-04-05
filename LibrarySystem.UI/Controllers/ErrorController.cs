using Microsoft.AspNetCore.Mvc;

namespace LibrarySystem.UI.Controllers
{
    public class ErrorController : Controller
    {
        [Route("Error")]
        public IActionResult Index(string? message)
        {
            ViewBag.ErrorMessage = message ?? "An unexpected error occurred. Our engineers have been notified.";
            return View();
        }
    }
}
