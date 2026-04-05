namespace LibrarySystem.Core.Entities
{
    public class User : BaseAuditableEntity
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty; // 'Admin' or 'User'
        public bool IsActive { get; set; }
    }
}
