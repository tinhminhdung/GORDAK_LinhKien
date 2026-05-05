using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Antigravity.ECommerce.Models;
using Antigravity.ECommerce.Services;
using System.Collections.Generic;
using System.Linq;
using Antigravity.ECommerce.Framework;

namespace Antigravity.ECommerce.Controllers
{
    [Authorize]
    [Permission("News", ActionType.View)]
    public class AdminNewsController : Controller
    {
        public IActionResult Index(string kw = "", int? catId = null, int? status = null, string sort = "CreatedAt", string order = "DESC", int page = 1, int size = 20)
        {
            var list = SNews.Search(kw, catId, status, sort, order, page, size);
            ViewBag.Keyword = kw;
            ViewBag.CategoryId = catId;
            ViewBag.Status = status;
            ViewBag.SortColumn = sort;
            ViewBag.SortOrder = order;
            ViewBag.PageIndex = page;
            ViewBag.PageSize = size;
            ViewBag.TotalCount = list.Count > 0 ? list[0].TotalCount : 0;
            
            ViewBag.Categories = SCategory.GetHierarchical().Where(x => x.CategoryType == 2).ToList();
            return View(list);
        }


        [Permission("News", ActionType.Create)]
        public IActionResult Create()
        {
            ViewBag.Categories = SCategory.GetHierarchical().Where(x => x.CategoryType == 2).ToList();
            return View(new News { Status = 1, SortOrder = 0 });
        }

        [HttpPost]
        [Permission("News", ActionType.Create)]
        public IActionResult Create(News model)
        {
            if (ModelState.IsValid)
            {
                // Check duplicate Slug
                var existSlug = SNews.GetBySlug(model.Slug);
                if (existSlug != null)
                {
                    ModelState.AddModelError("Slug", $"Đường dẫn (Slug) '{model.Slug}' đã được sử dụng bởi bài viết '{existSlug.Title}'. VUi lòng thay đổi.");
                }

                // Check duplicate Title
                var existTitle = SNews.Search(model.Title, null, null, "CreatedAt", "DESC", 1, 10).FirstOrDefault(x => x.Title == model.Title);
                if (existTitle != null)
                {
                    ModelState.AddModelError("Title", "Tiêu đề bài viết này đã tồn tại trong hệ thống.");
                }

                if (ModelState.IsValid)
                {
                    model.CreatedBy = User.Identity?.Name ?? "Admin";
                    SNews.Insert(model);
                    SSeo.RefreshSitemapAndCache();
                    return RedirectToAction("Index");
                }
            }
            ViewBag.Categories = SCategory.GetHierarchical().Where(x => x.CategoryType == 2).ToList();
            return View(model);
        }


        [Permission("News", ActionType.Edit)]
        public IActionResult Edit(int id)
        {
            var item = SNews.GetById(id);
            if (item == null) return NotFound();

            ViewBag.Categories = SCategory.GetHierarchical().Where(x => x.CategoryType == 2).ToList();
            return View(item);
        }

        [HttpPost]
        [Permission("News", ActionType.Edit)]
        public IActionResult Edit(News model)
        {
            if (ModelState.IsValid)
            {
                // Check duplicate Slug
                var existSlug = SNews.GetBySlug(model.Slug);
                if (existSlug != null && existSlug.NewsId != model.NewsId)
                {
                    ModelState.AddModelError("Slug", $"Đường dẫn (Slug) '{model.Slug}' đã được sử dụng bởi bài viết '{existSlug.Title}'. Vui lòng thay đổi.");
                }

                // Check duplicate Title
                var existTitle = SNews.Search(model.Title, null, null, "CreatedAt", "DESC", 1, 10).FirstOrDefault(x => x.Title == model.Title && x.NewsId != model.NewsId);
                if (existTitle != null)
                {
                    ModelState.AddModelError("Title", "Tiêu đề bài viết này đã tồn tại.");
                }

                if (ModelState.IsValid)
                {
                    model.UpdatedBy = User.Identity?.Name ?? "Admin";
                    SNews.Update(model);
                    SSeo.RefreshSitemapAndCache();
                    return RedirectToAction("Index");
                }
            }
            ViewBag.Categories = SCategory.GetHierarchical().Where(x => x.CategoryType == 2).ToList();
            return View(model);
        }

        [HttpPost]
        [Permission("News", ActionType.Delete)]
        public IActionResult Delete(int id)
        {
            var result = SNews.Delete(id);
            if (result > 0) {
                SSeo.RefreshSitemapAndCache();
                return Json(new { success = true });
            }
            return Json(new { success = false, message = "Xóa thất bại" });
        }

        [HttpPost]
        [Permission("News", ActionType.Delete)]
        public IActionResult BulkDelete([FromBody] List<int> ids)
        {
            if (ids == null || !ids.Any()) return Json(new { success = false, message = "Không có mục nào được chọn" });
            int result = SNews.BulkDelete(ids);
            if (result > 0) {
                SSeo.RefreshSitemapAndCache();
                return Json(new { success = true, count = result });
            }
            return Json(new { success = false, message = "Xóa thất bại" });
        }

        [HttpPost]
        [Permission("News", ActionType.Edit)]
        public IActionResult BulkUpdateStatus([FromBody] BulkStatusModel model)
        {
            if (model.Ids == null || !model.Ids.Any()) return Json(new { success = false });
            int result = SNews.BulkUpdateStatus(model.Ids, model.Status);
            if (result > 0) SSeo.RefreshSitemapAndCache();
            return Json(new { success = result > 0 });
        }

        [HttpPost]
        [Permission("News", ActionType.Edit)]
        public IActionResult UpdateQuick(int id, string field, bool value)
        {
            // Allowed fields for quick update
            if (field != "IsHot") return Json(new { success = false, message = "Trường không hợp lệ" });

            try
            {
                // Dùng ExecuteNonQuery để update nhanh field
                int val = value ? 1 : 0;
                string sql = $"UPDATE News SET {field} = {val} WHERE NewsId = {id}";
                Antigravity.ECommerce.Services.BaseConnectionSql.ExecuteNonQuery(sql);
                SSeo.RefreshSitemapAndCache();
                return Json(new { success = true });
            }
            catch (System.Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public class BulkStatusModel {
            public List<int> Ids { get; set; } = new();
            public int Status { get; set; }
        }
    }
}
