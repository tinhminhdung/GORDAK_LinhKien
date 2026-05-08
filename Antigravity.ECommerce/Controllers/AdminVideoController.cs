using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Antigravity.ECommerce.Models;
using Antigravity.ECommerce.Services;
using System.Linq;
using System.Collections.Generic;
using Antigravity.ECommerce.Framework;

namespace Antigravity.ECommerce.Controllers
{
    [Authorize]
    [Permission("Video", ActionType.View)]
    public class AdminVideoController : Controller
    {
        private void LoadCategories()
        {
            ViewBag.Categories = SCategory.GetHierarchical().Where(x => x.CategoryType == 3).ToList();
        }

        public IActionResult Index(string kw = "", int? status = null, int? categoryId = null, string sort = "CreatedAt", string order = "DESC", int page = 1, int size = 20)
        {
            if (page < 1) page = 1;
            var list = SVideo.Search(kw, status, categoryId, sort, order, page, size);
            ViewBag.SortColumn = sort;
            ViewBag.SortOrder = order;
            LoadCategories();
            ViewBag.Keyword = kw;
            ViewBag.Status = status;
            ViewBag.CategoryId = categoryId;
            ViewBag.PageIndex = page;
            ViewBag.PageSize = size;
            ViewBag.TotalCount = list.Count > 0 ? list[0].TotalCount : 0;
            return View(list);
        }


        [Permission("Video", ActionType.Create)]
        public IActionResult Create()
        {
            LoadCategories();
            return View(new Video { SortOrder = 0, Status = 1 });
        }

        [HttpPost]
        [Permission("Video", ActionType.Create)]
        public IActionResult Create(Video model)
        {
            if (ModelState.IsValid)
            {
                model.CreatedBy = User.Identity?.Name ?? "Admin";
                SVideo.Insert(model);
                SSeo.RefreshSitemapAndCache();
                return RedirectToAction("Index");
            }
            LoadCategories();
            return View(model);
        }


        [Permission("Video", ActionType.Edit)]
        public IActionResult Edit(int id)
        {
            var item = SVideo.GetById(id);
            if (item == null) return NotFound();
            LoadCategories();
            return View(item);
        }

        [HttpPost]
        [Permission("Video", ActionType.Edit)]
        public IActionResult Edit(Video model)
        {
            if (ModelState.IsValid)
            {
                model.UpdatedBy = User.Identity?.Name ?? "Admin";
                SVideo.Update(model);
                SSeo.RefreshSitemapAndCache();
                return RedirectToAction("Index");
            }
            LoadCategories();
            return View(model);
        }

        [HttpPost]
        [Permission("Video", ActionType.Delete)]
        public IActionResult Delete(int id)
        {
            var result = SVideo.Delete(id);
            if (result > 0) {
                SSeo.RefreshSitemapAndCache();
                return Json(new { success = true });
            }
            return Json(new { success = false, message = "Xóa thất bại" });
        }

        [HttpPost]
        [Permission("Video", ActionType.Delete)]
        public IActionResult BulkDelete([FromBody] List<int> ids)
        {
            if (ids == null || ids.Count == 0) return Json(new { success = false });
            int result = SVideo.BulkDelete(ids);
            if (result > 0) SSeo.RefreshSitemapAndCache();
            return Json(new { success = result > 0 });
        }

        [HttpPost]
        [Permission("Video", ActionType.Edit)]
        public IActionResult BulkUpdateStatus([FromBody] BulkUpdateModel model)
        {
            if (model.Ids == null || model.Ids.Count == 0) return Json(new { success = false });
            int result = SVideo.BulkUpdateStatus(model.Ids, model.Status);
            if (result > 0) SSeo.RefreshSitemapAndCache();
            return Json(new { success = result > 0 });
        }

        /// <summary> Bật/tắt nhanh IsHot cho Video ngay tại trang danh sách </summary>
        [HttpPost]
        [Permission("Video", ActionType.Edit)]
        public IActionResult ToggleIsHot(int id, bool value)
        {
            bool ok = SVideo.ToggleIsHot(id, value);
            return Json(new { success = ok });
        }

        public class BulkUpdateModel {
            public List<int> Ids { get; set; } = new();
            public int Status { get; set; }
        }
    }
}
