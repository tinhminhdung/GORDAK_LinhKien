using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Linq;
using Antigravity.ECommerce.Models;
using System.Collections.Generic;

namespace Antigravity.ECommerce.Framework
{
    public static class PermissionHelper
    {
        // Kiểm tra xem người dùng hiện tại có quyền Xem cho module hay không
        public static bool CanView(HttpContext context, string module)
        {
            var user = context.User;
            if (user == null || user.Identity?.IsAuthenticated != true) return false;

            var roleIdClaim = user.Claims.FirstOrDefault(c => c.Type == "RoleId")?.Value;
            if (string.IsNullOrEmpty(roleIdClaim) || !int.TryParse(roleIdClaim, out int roleId)) return false;

            // Super Admin - Luôn cho phép module AdminRoles
            if (roleId == 1 && module.Equals("AdminRoles", StringComparison.OrdinalIgnoreCase)) return true;


            // Kiểm tra cache hoặc gọi DB (Sử dụng FAdminUser đã có)
            var permissions = FAdminUser.GetPermissions(roleId);
            var p = permissions.FirstOrDefault(x => x.ModuleName == module);
            return p != null && p.CanView;
        }

        // Kiểm tra quyền bất kỳ (CanCreate, CanEdit, CanDelete, CanExport)
        public static bool HasPermission(HttpContext context, string module, string action)
        {
            var user = context.User;
            if (user == null || user.Identity?.IsAuthenticated != true) return false;

            var roleIdClaim = user.Claims.FirstOrDefault(c => c.Type == "RoleId")?.Value;
            if (string.IsNullOrEmpty(roleIdClaim) || !int.TryParse(roleIdClaim, out int roleId)) return false;

            if (roleId == 1 && module.Equals("AdminRoles", StringComparison.OrdinalIgnoreCase)) return true;


            var permissions = FAdminUser.GetPermissions(roleId);
            var p = permissions.FirstOrDefault(x => x.ModuleName == module);
            if (p == null) return false;

            return action switch
            {
                "CanView" => p.CanView,
                "CanCreate" => p.CanCreate,
                "CanEdit" => p.CanEdit,
                "CanDelete" => p.CanDelete,
                "CanExport" => p.CanExport,
                _ => false
            };
        }
    }
}
