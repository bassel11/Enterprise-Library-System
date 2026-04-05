using Microsoft.Data.SqlClient;

namespace LibrarySystem.UI.Middlewares
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "A critical database error occurred.");
                await HandleExceptionAsync(context, "An error occurred while connecting to the database. Please try again later.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred.");
                await HandleExceptionAsync(context, "An unexpected error occurred. Our team has been notified.");
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, string message)
        {
            context.Response.Redirect($"/Home/Error?message={Uri.EscapeDataString(message)}");
            return Task.CompletedTask;
        }
    }
}
