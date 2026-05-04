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
    [Permission("FAQ", ActionType.View)]
    public class AdminFAQController : Controller
    {
        private void LoadCategories()
        {
            ViewBag.Categories = SCategory.GetHierarchical().Where(x => x.CategoryType == 7).ToList();
        }

        public IActionResult Index(string kw = "", int? status = null, int? categoryId = null, string sort = "CreatedAt", string order = "DESC", int page = 1, int size = 20)
        {
            if (page < 1) page = 1;
            var list = SFAQ.Search(kw, status, categoryId, sort, order, page, size);
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


        [Permission("FAQ", ActionType.Create)]
        public IActionResult Create()
        {
            LoadCategories();
            return View(new FAQ { SortOrder = 0, Status = 1 });
        }

        [HttpPost]
        [Permission("FAQ", ActionType.Create)]
        public IActionResult Create(FAQ model)
        {
            ModelState.Clear();
            if (string.IsNullOrWhiteSpace(model.Question) || string.IsNullOrWhiteSpace(model.Answer))
            {
                ModelState.AddModelError("", "Vui lòng nhập đầy đủ Câu hỏi và Câu trả lời chi tiết.");
                LoadCategories();
                return View(model);
            }

            try
            {
                model.CreatedBy = User.Identity?.Name ?? "Admin";
                SFAQ.Insert(model);
                SSeo.RefreshSitemapAndCache();
                return RedirectToAction("Index");
            }
            catch (System.Exception ex)
            {
                ModelState.AddModelError("", "Lỗi lưu Database: " + ex.Message);
            }
            
            LoadCategories();
            return View(model);
        }


        [Permission("FAQ", ActionType.Edit)]
        public IActionResult Edit(int id)
        {
            var item = SFAQ.GetById(id);
            if (item == null) return NotFound();
            LoadCategories();
            return View(item);
        }

        [HttpPost]
        [Permission("FAQ", ActionType.Edit)]
        public IActionResult Edit(FAQ model)
        {
            ModelState.Clear();
            if (string.IsNullOrWhiteSpace(model.Question) || string.IsNullOrWhiteSpace(model.Answer))
            {
                ModelState.AddModelError("", "Vui lòng nhập đầy đủ Câu hỏi và Câu trả lời chi tiết.");
                LoadCategories();
                return View(model);
            }

            try
            {
                model.UpdatedBy = User.Identity?.Name ?? "Admin";
                SFAQ.Update(model);
                SSeo.RefreshSitemapAndCache();
                return RedirectToAction("Index");
            }
            catch (System.Exception ex)
            {
                ModelState.AddModelError("", "Lỗi lưu Database: " + ex.Message);
            }
            
            LoadCategories();
            return View(model);
        }

        [HttpPost]
        [Permission("FAQ", ActionType.Delete)]
        public IActionResult Delete(int id)
        {
            var result = SFAQ.Delete(id);
            if (result > 0) {
                SSeo.RefreshSitemapAndCache();
                return Json(new { success = true });
            }
            return Json(new { success = false, message = "Xóa thất bại" });
        }

        [HttpPost]
        [Permission("FAQ", ActionType.Delete)]
        public IActionResult BulkDelete([FromBody] List<int> ids)
        {
            if (ids == null || ids.Count == 0) return Json(new { success = false });
            int result = SFAQ.BulkDelete(ids);
            if (result > 0) SSeo.RefreshSitemapAndCache();
            return Json(new { success = result > 0 });
        }

        [HttpPost]
        [Permission("FAQ", ActionType.Edit)]
        public IActionResult BulkUpdateStatus([FromBody] BulkUpdateModel model)
        {
            if (model.Ids == null || model.Ids.Count == 0) return Json(new { success = false });
            int result = SFAQ.BulkUpdateStatus(model.Ids, model.Status);
            if (result > 0) SSeo.RefreshSitemapAndCache();
            return Json(new { success = result > 0 });
        }

        public class BulkUpdateModel
        {
            public List<int> Ids { get; set; } = new();
            public int Status { get; set; }
        }
    }
}
