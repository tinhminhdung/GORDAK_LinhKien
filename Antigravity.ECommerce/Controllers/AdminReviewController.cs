using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Antigravity.ECommerce.Framework;
using Antigravity.ECommerce.Models;
using System;

namespace Antigravity.ECommerce.Controllers
{
    [Authorize]
    [Permission("Review", ActionType.View)]
    public class AdminReviewController : Controller
    {
        public IActionResult Index(string? kw, int? rating, int? status, string sort = "CreatedAt", string order = "DESC", int page = 1, int size = 20)
        {
            var reviews = FReview.SearchAdmin(kw, rating, status, sort, order, page, size);
            ViewBag.Keyword = kw;
            ViewBag.Rating = rating;
            ViewBag.Status = status;
            ViewBag.SortColumn = sort;
            ViewBag.SortOrder = order;
            ViewBag.PageIndex = page;
            ViewBag.PageSize = size;
            ViewBag.TotalCount = reviews.Count > 0 ? reviews[0].TotalCount : 0;
            ViewBag.TotalPages = (int)Math.Ceiling((double)ViewBag.TotalCount / size);
            return View(reviews);
        }

        [HttpPost]
        [Permission("Review", ActionType.Edit)]
        public IActionResult Reply(int reviewId, string reply)
        {
            if (string.IsNullOrWhiteSpace(reply))
                return Json(new { success = false, message = "Nội dung phản hồi không được để trống" });

            string adminUser = User.Identity?.Name ?? "admin";
            FReview.AdminReply(reviewId, reply, adminUser);
            return Json(new { success = true, message = "Đã gửi phản hồi thành công" });
        }

        [HttpPost]
        [Permission("Review", ActionType.Edit)]
        public IActionResult ToggleHide(int reviewId)
        {
            FReview.ToggleStatus(reviewId);
            return Json(new { success = true, message = "Đã cập nhật trạng thái" });
        }

        [HttpPost]
        [Permission("Review", ActionType.Delete)]
        public IActionResult Delete(int reviewId)
        {
            FReview.Delete(reviewId);
            return Json(new { success = true, message = "Đã xóa đánh giá" });
        }
    }
}
