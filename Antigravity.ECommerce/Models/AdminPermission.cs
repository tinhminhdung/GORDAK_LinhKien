using System;

namespace Antigravity.ECommerce.Models
{
    public class AdminPermission
    {
        public int PermissionId { get; set; }
        public int RoleId { get; set; }
        public string ModuleName { get; set; } = string.Empty;
        public bool CanView { get; set; }
        public bool CanCreate { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
        public bool CanExport { get; set; }
    }
}
