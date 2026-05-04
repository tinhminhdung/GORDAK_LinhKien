using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Antigravity.ECommerce.Models;
using Antigravity.ECommerce.Services;
using Antigravity.ECommerce.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Antigravity.ECommerce.Controllers
{
    [Authorize]
    [Permission("Gallery")]
    public class AdminGalleryCategoryController : Controller
    {
        private const int CategoryTypeCode = 6;

        public IActionResult Index(string kw = "", int? status = null, int page = 1, int size = 20)
        {
            var all = SCategory.GetAll().Where(x => x.CategoryType == CategoryTypeCode).ToList();
            if (!string.IsNullOrEmpty(kw))
                all = all.Where(x => x.Name.Contains(kw, StringComparison.OrdinalIgnoreCase)).ToList();
            if (status.HasValue)
                all = all.Where(x => x.Status == status.Value).ToList();

            var roots = all.Where(x => x.ParentId == 0).OrderBy(x => x.SortOrder).ToList();
            int totalRoots = roots.Count;
            var pagedRoots = roots.Skip((page - 1) * size).Take(size).ToList();
            foreach (var r in pagedRoots) 
                r.Children = SCategory.BuildTree(all, r.CategoryId);

            ViewBag.Keyword = kw;
            ViewBag.Status = status;
            ViewBag.TotalCount = all.Count;
            ViewBag.PageIndex = page;
            ViewBag.PageSize = size;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalRoots / size);
            return View(pagedRoots);
        }

        public IActionResult Create(int? parentId = null)
        {
            ViewBag.Categories = SCategory.GetHierarchical().Where(x => x.CategoryType == CategoryTypeCode).ToList();
            return View(new Category { CategoryType = CategoryTypeCode, Status = 1, ParentId = parentId ?? 0 });
        }

        [HttpPost]
        public IActionResult Create(Category model)
        {
            model.CategoryType = CategoryTypeCode;
            if (ModelState.IsValid)
            {
                var existing = FCategory.GetBySlug(model.Slug);
                if (existing != null)
                    ModelState.AddModelError("Slug", "Slug này đã tồn tại.");

                var existingName = SCategory.Search(model.Name, null, null, "Name", "ASC", 1, 10, CategoryTypeCode).FirstOrDefault(x => x.Name == model.Name);
                if (existingName != null)
                    ModelState.AddModelError("Name", "Tên danh mục thư viện ảnh này đã tồn tại.");

                if (ModelState.IsValid)
                {
                    model.CreatedBy = "Admin";
                    SCategory.Insert(model);
                    return RedirectToAction("Index");
                }
            }
            ViewBag.Categories = SCategory.GetHierarchical().Where(x => x.CategoryType == CategoryTypeCode).ToList();
            return View(model);
        }

        public IActionResult Edit(int id)
        {
            var category = SCategory.GetById(id);
            if (category == null || category.CategoryType != CategoryTypeCode) return NotFound();
            ViewBag.Categories = SCategory.GetHierarchical().Where(x => x.CategoryType == CategoryTypeCode).ToList();
            return View(category);
        }

        [HttpPost]
        public IActionResult Edit(Category model)
        {
            model.CategoryType = CategoryTypeCode;
            if (ModelState.IsValid)
            {
                var existing = FCategory.GetBySlug(model.Slug);
                if (existing != null && existing.CategoryId != model.CategoryId)
                    ModelState.AddModelError("Slug", "Slug này đã được sử dụng.");

                var existingName = SCategory.Search(model.Name, null, null, "Name", "ASC", 1, 10, CategoryTypeCode).FirstOrDefault(x => x.Name == model.Name && x.CategoryId != model.CategoryId);
                if (existingName != null)
                    ModelState.AddModelError("Name", "Tên danh mục thư viện ảnh này đã tồn tại.");

                if (ModelState.IsValid)
                {
                    model.UpdatedBy = "Admin";
                    SCategory.Update(model);
                    return RedirectToAction("Index");
                }
            }
            ViewBag.Categories = SCategory.GetHierarchical().Where(x => x.CategoryType == CategoryTypeCode).ToList();
            return View(model);
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            try
            {
                var item = SCategory.GetById(id);
                if (item != null && item.CategoryType == CategoryTypeCode)
                {
                    SCategory.Delete(id);
                    return Json(new { success = true });
                }
                return Json(new { success = false, message = "Không tìm thấy danh mục" });
            }
            catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
        }

        [HttpPost]
        public IActionResult BulkDelete([FromBody] List<int> ids)
        {
            try
            {
                var validIds = new List<int>();
                foreach (var id in ids)
                {
                    var item = SCategory.GetById(id);
                    if (item != null && item.CategoryType == CategoryTypeCode) validIds.Add(id);
                }
                SCategory.BulkDelete(validIds);
                return Json(new { success = true });
            }
            catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
        }

        [HttpPost]
        public IActionResult UpdateQuick(int id, int? status, int? sortOrder)
        {
            var result = SCategory.UpdateQuick(id, status, sortOrder);
            if (result > 0) return Json(new { success = true });
            return Json(new { success = false });
        }

        [HttpPost]
        public IActionResult BulkUpdateStatus([FromBody] BulkStatusModel model)
        {
            if (model.Ids == null || !model.Ids.Any()) return Json(new { success = false });
            var updated = SCategory.BulkUpdateQuick(model.Ids, model.Status, null);
            return Json(new { success = updated > 0 });
        }

        public class BulkStatusModel {
            public List<int> Ids { get; set; } = new();
            public int Status { get; set; }
        }
    }
}
