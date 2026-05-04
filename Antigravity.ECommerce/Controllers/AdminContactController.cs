using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Antigravity.ECommerce.Models;
using Antigravity.ECommerce.Framework;
using Antigravity.ECommerce.Services;

namespace Antigravity.ECommerce.Controllers
{
    [Authorize]
    [Permission("Contact", ActionType.View)]
    public class AdminContactController : Controller
    {
        public IActionResult Index(string kw = "", int? status = null, string? dateFilter = null, int page = 1)
        {
            int pageSize = 20;
            var list = FContact.Search(kw, status, dateFilter, "CreatedAt", "DESC", page, pageSize);
            
            // Tính số trang
            int totalRecords = FContact.GetTotalCount(kw, status, dateFilter);
            int totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

            // Lấy thống kê cho Dashboard
            ViewBag.Stats = FContact.GetStats();
            ViewBag.CurrentKw = kw;
            ViewBag.CurrentStatus = status;
            ViewBag.CurrentDate = dateFilter;
            ViewBag.Page = page;
            ViewBag.TotalPages = totalPages;

            return View(list);
        }

        [HttpGet]
        [Permission("Contact", ActionType.View)]
        public IActionResult GetDetails(int id)
        {
            var msg = FContact.GetById(id);
            if (msg == null) return NotFound();

            // Nếu tin nhắn mới (Status = 0), đánh dấu là Đã đọc (Status = 1)
            if (msg.Status == 0)
            {
                msg.Status = 1;
                msg.ReadAt = DateTime.Now;
                msg.UpdatedBy = User.Identity?.Name ?? "Admin";
                FContact.Update(msg);
            }

            return Json(new { success = true, data = msg });
        }

        [HttpPost]
        [Permission("Contact", ActionType.Edit)]
        public IActionResult UpdateNote(int id, string note)
        {
            var msg = FContact.GetById(id);
            if (msg == null) return Json(new { success = false });

            msg.AdminNote = note;
            msg.UpdatedBy = User.Identity?.Name ?? "Admin";
            FContact.Update(msg);

            return Json(new { success = true });
        }

        [HttpPost]
        [Permission("Contact", ActionType.Edit)]
        public IActionResult UpdateStatus(int id, int status)
        {
            var msg = FContact.GetById(id);
            if (msg == null) return Json(new { success = false });

            msg.Status = status;
            msg.UpdatedBy = User.Identity?.Name ?? "Admin";
            FContact.Update(msg);

            return Json(new { success = true });
        }

        [HttpPost]
        [Permission("Contact", ActionType.Edit)]
        public IActionResult ToggleStar(int id)
        {
            var msg = FContact.GetById(id);
            if (msg == null) return Json(new { success = false });

            msg.IsStarred = !msg.IsStarred;
            msg.UpdatedBy = User.Identity?.Name ?? "Admin";
            FContact.Update(msg);

            return Json(new { success = true, isStarred = msg.IsStarred });
        }

        [HttpPost]
        [Permission("Contact", ActionType.Delete)]
        public IActionResult Delete(int id)
        {
            var result = FContact.Delete(id);
            return Json(new { success = result > 0 });
        }
    }
}
