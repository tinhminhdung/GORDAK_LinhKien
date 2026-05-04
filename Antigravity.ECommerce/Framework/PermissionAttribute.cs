using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Antigravity.ECommerce.Models;
using Antigravity.ECommerce.Services;
using System.Net;

namespace Antigravity.ECommerce.Framework
{
    public enum ActionType
    {
        View,
        Create,
        Edit,
        Delete,
        Export
    }

    public class PermissionAttribute : ActionFilterAttribute
    {
        public string Module { get; }
        public ActionType Action { get; }

        public PermissionAttribute(string module, ActionType action = ActionType.View)
        {
            Module = module;
            Action = action;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var user = context.HttpContext.User;
            if (user.Identity?.IsAuthenticated != true)
            {
                context.Result = new RedirectToActionResult("Login", "Admin", null);
                return;
            }
            // Kiểm tra trạng thái tài khoản thực tế trong DB
            var userIdClaim = user.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out int userId))
            {
                var statusObj = BaseConnectionSql.ExecuteScalar("SELECT Status FROM AdminUsers WHERE UserId = @Id", new Microsoft.Data.SqlClient.SqlParameter("@Id", userId));
                int status = statusObj != null ? Convert.ToInt32(statusObj) : 0;
                
                if (status <= 0)
                {
                    // Đẩy về trang Login bằng URL chuỗi để tránh lỗi mã hóa của RedirectToActionResult
                    string errorMsg = WebUtility.UrlEncode("Tài khoản của bạn đã bị khóa hoặc bị xóa!");
                    context.Result = new RedirectResult($"/Admin/Login?error={errorMsg}");
                    return;
                }
            }

            var roleIdClaim = user.Claims.FirstOrDefault(c => c.Type == "RoleId")?.Value;
            if (string.IsNullOrEmpty(roleIdClaim) || !int.TryParse(roleIdClaim, out int roleId))
            {
                context.Result = new ForbidResult();
                return;
            }

            // 1. Super Admin (RoleId = 1) - Toàn quyền (Ngoại trừ khi test hoặc muốn tự khóa)
            // CHÚ Ý: Luôn cho phép vào module AdminRoles để không bị khóa vĩnh viễn
            if (roleId == 1 && Module.Equals("AdminRoles", StringComparison.OrdinalIgnoreCase))
            {
                base.OnActionExecuting(context);
                return;
            }


            // 2. Kiểm tra quyền từ CSDL
            var permissions = FAdminUser.GetPermissions(roleId);
            var perm = permissions.FirstOrDefault(p => p.ModuleName.Equals(Module, StringComparison.OrdinalIgnoreCase));

            bool hasPermission = false;
            if (perm != null)
            {
                hasPermission = Action switch
                {
                    ActionType.View => perm.CanView,
                    ActionType.Create => perm.CanCreate,
                    ActionType.Edit => perm.CanEdit,
                    ActionType.Delete => perm.CanDelete,
                    ActionType.Export => perm.CanExport,
                    _ => false
                };
            }

            if (!hasPermission)
            {
                // Nếu là AJAX request, trả về JSON
                if (context.HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    context.Result = new JsonResult(new { success = false, message = "Bạn không có quyền thực hiện thao tác này!" });
                }
                else
                {
                    // Trả về trang 403 chuyên nghiệp
                    context.Result = new RedirectToActionResult("AccessDenied", "Admin", new { module = Module });
                }
            }

            base.OnActionExecuting(context);
        }

        public override void OnActionExecuted(ActionExecutedContext context)
        {
            base.OnActionExecuted(context);

            // Ghi nhận Activity Log nếu sửa/xóa/thêm thành công
            if (context.Exception == null && (Action == ActionType.Create || Action == ActionType.Edit || Action == ActionType.Delete || Action == ActionType.Export))
            {
                var method = context.HttpContext.Request.Method;
                // Chỉ log khi thực sự submit dữ liệu (POST, DELETE, PUT) hoặc xuất Export
                if (method.Equals("POST", StringComparison.OrdinalIgnoreCase) || 
                    method.Equals("DELETE", StringComparison.OrdinalIgnoreCase) || 
                    method.Equals("PUT", StringComparison.OrdinalIgnoreCase) ||
                    Action == ActionType.Export)
                {
                    string username = context.HttpContext.User.Identity?.Name ?? "Unknown";
                    string ip = context.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "";
                    string endpoint = context.HttpContext.Request.Path;
                    
                    var log = new AdminLog
                    {
                        Username = username,
                        Action = Action.ToString(),
                        Module = Module,
                        Description = $"Thực hiện thao tác '{Action}' thành công tại đường dẫn {endpoint}",
                        IpAddress = ip
                    };
                    
                    try { FAdminLog.Insert(log); } catch { /* Ignore log failure */ }
                }
            }
        }
    }
}
