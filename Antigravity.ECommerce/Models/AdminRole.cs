using System;

namespace Antigravity.ECommerce.Models
{
    public class AdminRole : BaseModel
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Color { get; set; } = "primary";
        public bool IsSystem { get; set; }
    }
}
