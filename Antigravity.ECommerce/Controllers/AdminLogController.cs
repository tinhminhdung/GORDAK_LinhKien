using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Antigravity.ECommerce.Framework;
using System;

namespace Antigravity.ECommerce.Controllers
{
    [Authorize]
    [Permission("AdminLogs", ActionType.View)]
    public class AdminLogController : Controller
    {
        public IActionResult Index(string module = "", string username = "", string actionType = "", string fromDate = "", string toDate = "", int page = 1)
        {
            int pageSize = 50;
            var logs = FAdminLog.Search(module, username, actionType, fromDate, toDate, page, pageSize);
            int total = FAdminLog.GetTotalCount(module, username, actionType, fromDate, toDate);
            
            ViewBag.CurrentModule = module;
            ViewBag.CurrentUsername = username;
            ViewBag.CurrentAction = actionType;
            ViewBag.FromDate = fromDate;
            ViewBag.ToDate = toDate;
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalCount = total;
            ViewBag.TotalPages = (int)Math.Ceiling((double)total / pageSize);

            ViewBag.Modules = FAdminLog.GetDistinctModules();
            ViewBag.Usernames = FAdminLog.GetDistinctUsernames();

            return View(logs);
        }

        [HttpPost]
        [Permission("AdminLogs", ActionType.Delete)]
        public IActionResult ClearOldLogs(int days = 30)
        {
            try
            {
                int rows = FAdminLog.ClearOldLogs(days);
                return Json(new { success = true, message = $"Đã xóa {rows} bản ghi lịch sử cũ hơn {days} ngày." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        [HttpPost]
        [Permission("AdminLogs", ActionType.Delete)]
        public IActionResult ClearAllLogs()
        {
            try
            {
                FAdminLog.ClearAllLogs();
                return Json(new { success = true, message = "Đã dọn dẹp sạch toàn bộ lịch sử (Truncate table)." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }
    }
}
