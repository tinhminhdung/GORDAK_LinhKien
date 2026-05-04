using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Antigravity.ECommerce.Models;
using Antigravity.ECommerce.Framework;
using Antigravity.ECommerce.Services;
using Microsoft.Data.SqlClient;

namespace Antigravity.ECommerce.Controllers
{
    [Authorize]
    [Permission("AdminRoles", ActionType.View)]
    public class AdminRoleController : Controller
    {
        [HttpPost]
        [Permission("AdminRoles", ActionType.Create)]
        public IActionResult Create(string name, string desc, string color)
        {
            if (string.IsNullOrEmpty(name)) return Json(new { success = false, message = "Tên vai trò không được để trống" });
            
            var sql = "INSERT INTO AdminRoles (RoleName, Description, Color, IsSystem) VALUES (@Name, @Desc, @Color, 0)";
            var prm = new Microsoft.Data.SqlClient.SqlParameter[] {
                new Microsoft.Data.SqlClient.SqlParameter("@Name", name),
                new Microsoft.Data.SqlClient.SqlParameter("@Desc", desc ?? (object)DBNull.Value),
                new Microsoft.Data.SqlClient.SqlParameter("@Color", color ?? "primary")
            };
            
            int res = BaseConnectionSql.ExecuteNonQuery(sql, prm);
            return Json(new { success = res > 0 });
        }

        public IActionResult Index()
        {
            var list = BaseConnectionSql.Query<AdminRole>("SELECT * FROM AdminRoles");
            return View(list);
        }


        [Permission("AdminRoles", ActionType.Edit)]
        public IActionResult Permissions(int id)
        {
            var role = BaseConnectionSql.QuerySingle<AdminRole>("SELECT * FROM AdminRoles WHERE RoleId = @id", new SqlParameter("@id", id));
            if (role == null) return NotFound();

            var permissions = FAdminUser.GetPermissions(id);
            ViewBag.Role = role;

            // Danh sách các module cần phân quyền
            var modules = new List<string> { 
                "Dashboard", "Products", "News", "Categories", "Video", "Gallery", 
                "FAQ", "Documents", "Advertising", "Location", "Contact", "SeoAudit", 
                "Settings", "Menu", "FileManager", "AdminUsers", "AdminRoles", "AdminLogs", "Customers", "Orders", "Review"
            };
            ViewBag.Modules = modules;

            return View(permissions);
        }

        [HttpPost]
        [Permission("AdminRoles", ActionType.Edit)]
        public IActionResult UpdatePermission(int roleId, string module, string action, bool value)
        {
            // Cập nhật hoặc chèn mới quyền
            string sql = $@"
                IF EXISTS (SELECT 1 FROM AdminPermissions WHERE RoleId = @RoleId AND ModuleName = @Module)
                BEGIN
                    UPDATE AdminPermissions SET {action} = @Value WHERE RoleId = @RoleId AND ModuleName = @Module
                END
                ELSE
                BEGIN
                    INSERT INTO AdminPermissions (RoleId, ModuleName, {action}) VALUES (@RoleId, @Module, @Value)
                END";

            var prm = new SqlParameter[] {
                new SqlParameter("@RoleId", roleId),
                new SqlParameter("@Module", module),
                new SqlParameter("@Value", value)
            };

            int res = BaseConnectionSql.ExecuteNonQuery(sql, prm);
            return Json(new { success = res > 0 });
        }
        [HttpPost]
        [Permission("AdminRoles", ActionType.Delete)]
        public IActionResult Delete(int id)
        {
            // Kiểm tra xem có user nào thuộc Role này không
            var userCount = Convert.ToInt32(BaseConnectionSql.ExecuteScalar("SELECT COUNT(*) FROM AdminUsers WHERE RoleId = @id", new SqlParameter("@id", id)));
            if (userCount > 0)
            {
                return Json(new { success = false, message = $"Không thể xóa. Hiện đang có {userCount} tài khoản thuộc vai trò này." });
            }

            var res = BaseConnectionSql.ExecuteNonQuery("DELETE FROM AdminRoles WHERE RoleId = @id AND IsSystem = 0", new SqlParameter("@id", id));
            return Json(new { success = res > 0 });
        }
    }
}
