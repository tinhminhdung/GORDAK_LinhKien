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
    [Permission("AdminUsers", ActionType.View)]
    public class AdminUserController : Controller
    {
        public IActionResult Index()
        {
            var list = FAdminUser.GetAll();
            return View(list);
        }


        [Permission("AdminUsers", ActionType.Create)]
        public IActionResult Create()
        {
            ViewBag.Roles = BaseConnectionSql.Query<AdminRole>("SELECT * FROM AdminRoles");
            return View(new AdminUser { Status = 1 });
        }

        [HttpPost]
        [Permission("AdminUsers", ActionType.Create)]
        public IActionResult Create(AdminUser model)
        {
            if (ModelState.IsValid)
            {
                // Simple validation checks
                bool hasError = false;
                if (string.IsNullOrEmpty(model.Password))
                {
                    ModelState.AddModelError("Password", "Mật khẩu không được để trống");
                    hasError = true;
                }
                if (!model.RoleId.HasValue || model.RoleId.Value <= 0)
                {
                    ModelState.AddModelError("RoleId", "Vui lòng chọn Vai trò hệ thống");
                    hasError = true;
                }

                if (!hasError)
                {
                    model.CreatedBy = User.Identity?.Name ?? "Admin";
                    FAdminUser.Insert(model);
                    return RedirectToAction("Index");
                }
            }
            ViewBag.Roles = BaseConnectionSql.Query<AdminRole>("SELECT * FROM AdminRoles");
            return View(model);
        }


        [Permission("AdminUsers", ActionType.Edit)]
        public IActionResult Edit(int id)
        {
            var user = FAdminUser.GetById(id);
            if (user == null) return NotFound();

            ViewBag.Roles = BaseConnectionSql.Query<AdminRole>("SELECT * FROM AdminRoles");
            return View(user);
        }

        [HttpPost]
        [Permission("AdminUsers", ActionType.Edit)]
        public IActionResult Edit(AdminUser model)
        {
            if (model.RoleId != 1 || model.Username != "admin") // Bỏ qua validate role cho admin gốc
            {
                if (!model.RoleId.HasValue || model.RoleId.Value <= 0)
                {
                    ModelState.AddModelError("RoleId", "Vui lòng chọn Vai trò hệ thống");
                    ViewBag.Roles = BaseConnectionSql.Query<AdminRole>("SELECT * FROM AdminRoles");
                    return View(model);
                }
            }

            model.UpdatedBy = User.Identity?.Name ?? "Admin";
            FAdminUser.Update(model);
            return RedirectToAction("Index");
        }

        [HttpPost]
        [Permission("AdminUsers", ActionType.Edit)]
        public IActionResult UpdateStatus(int id, int status)
        {
            var user = FAdminUser.GetById(id);
            if (user == null || user.RoleId == 1) return Json(new { success = false }); // Không cho khóa Super Admin

            var sql = "UPDATE AdminUsers SET Status = @Status WHERE UserId = @UserId";
            var res = BaseConnectionSql.ExecuteNonQuery(sql, new SqlParameter[] {
                new SqlParameter("@Status", status),
                new SqlParameter("@UserId", id)
            });
            return Json(new { success = res > 0 });
        }

        [HttpPost]
        [Permission("AdminUsers", ActionType.Delete)]
        public IActionResult Delete(int id)
        {
            int res = FAdminUser.Delete(id);
            return Json(new { success = res > 0 });
        }
    }
}
