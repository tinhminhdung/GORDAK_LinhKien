using System;

namespace Antigravity.ECommerce.Models
{
    public class AdminUser : BaseModel
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Avatar { get; set; }
        public int? RoleId { get; set; }
        public string? RoleName { get; set; } // Join field
        public int Status { get; set; } = 1;
        public DateTime? LastLogin { get; set; }
    }
}
