using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Antigravity.ECommerce.Models;
using Antigravity.ECommerce.Services;
using Antigravity.ECommerce.Framework;

namespace Antigravity.ECommerce.Controllers
{
    [Authorize]
    [Permission("Categories", ActionType.View)]
    public class AdminCategoryController : Controller
    {
        public IActionResult Index(int type = 1, string kw = "", int? status = null, string sort = "SortOrder", string order = "ASC", int page = 1, int size = 20)
        {
            var all = SCategory.GetAll().Where(x => x.CategoryType == type).ToList();
            ViewBag.Type = type;
            if (!string.IsNullOrEmpty(kw))
                all = all.Where(x => x.Name.Contains(kw, StringComparison.OrdinalIgnoreCase)).ToList();
            if (status.HasValue)
                all = all.Where(x => x.Status == status.Value).ToList();

            // Hàm sort dùng chung
            IEnumerable<Category> SortData(IEnumerable<Category> data) {
                if (sort == "Name") return order == "ASC" ? data.OrderBy(x => x.Name) : data.OrderByDescending(x => x.Name);
                if (sort == "CreatedAt") return order == "ASC" ? data.OrderBy(x => x.CreatedAt) : data.OrderByDescending(x => x.CreatedAt);
                return order == "ASC" ? data.OrderBy(x => x.SortOrder) : data.OrderByDescending(x => x.SortOrder);
            }

            // Build cây con bằng hàm tập trung (clone objects, không mutate cache)
            Func<IEnumerable<Category>, IEnumerable<Category>> sortFunc = SortData;

            // Paginate only ROOT items, children always shown
            var roots = SortData(all.Where(x => x.ParentId == 0)).ToList();
            ViewBag.SortColumn = sort;
            ViewBag.SortOrder = order;
            int totalRoots = roots.Count;
            var pagedRoots = roots.Skip((page - 1) * size).Take(size).ToList();
            
            // Dùng SCategory.BuildTree thay vì đệ quy inline (clone, an toàn, có giới hạn depth)
            foreach (var r in pagedRoots) 
                r.Children = SCategory.BuildTree(all, r.CategoryId, sortFunc);

            ViewBag.Keyword = kw;
            ViewBag.Status = status;
            ViewBag.TotalCount = all.Count;
            ViewBag.PageIndex = page;
            ViewBag.PageSize = size;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalRoots / size);
            ViewBag.HomeCategories = SHomeCategorySetting.GetAll();

            // Tính aggregate counts O(N) thay vì O(N²) cũ
            ViewBag.AggregateCounts = SCategory.ComputeAggregateCounts(all);

            return View(pagedRoots);
        }


        [Permission("Categories", ActionType.Create)]
        public IActionResult Create(int type = 1, int? parentId = null)
        {
            ViewBag.Categories = SCategory.GetHierarchical().Where(x => x.CategoryType == type).ToList();
            return View(new Category { CategoryType = type, Status = 1, ParentId = parentId ?? 0 });
        }

        [HttpPost]
        [Permission("Categories", ActionType.Create)]
        public IActionResult Create(Category model)
        {
            // model.CategoryType should be come from form hidden field
            if (ModelState.IsValid)
            {
                var existing = FCategory.GetBySlug(model.Slug);
                if (existing != null)
                {
                    ModelState.AddModelError("Slug", "Slug (URL chia sẻ) này đã tồn tại trong danh mục, vui lòng nhập slug khác để không bị trùng lặp đường dẫn.");
                }

                var existingName = SCategory.Search(model.Name, null, null, "Name", "ASC", 1, 10, model.CategoryType).FirstOrDefault(x => x.Name == model.Name);
                if (existingName != null)
                {
                    ModelState.AddModelError("Name", "Tên danh mục này đã tồn tại trong hệ thống.");
                }

                if (ModelState.IsValid)
                {
                    model.CreatedBy = User.Identity?.Name ?? "Admin";
                    SCategory.Insert(model);
                    // KHÔNG gọi SSeo.RefreshSitemapAndCache() ở đây - SCategory.Insert đã gọi rồi
                    return RedirectToAction("Index", new { type = model.CategoryType });
                }
            }
            ViewBag.Categories = SCategory.GetHierarchical().Where(x => x.CategoryType == model.CategoryType).ToList();
            return View(model);
        }


        [Permission("Categories", ActionType.Edit)]
        public IActionResult Edit(int id)
        {
            var category = SCategory.GetById(id);
            if (category == null) return NotFound();

            ViewBag.Categories = SCategory.GetHierarchical().Where(x => x.CategoryType == category.CategoryType).ToList();
            return View(category);
        }

        [HttpPost]
        [Permission("Categories", ActionType.Edit)]
        public IActionResult Edit(Category model)
        {
            // model.CategoryType should be come from form hidden field
            if (ModelState.IsValid)
            {
                var existing = FCategory.GetBySlug(model.Slug);
                if (existing != null && existing.CategoryId != model.CategoryId)
                {
                    ModelState.AddModelError("Slug", "Slug (URL chia sẻ) này đã tồn tại ở danh mục khác, vui lòng nhập slug khác để không bị trùng lặp đường dẫn.");
                }

                var existingName = SCategory.Search(model.Name, null, null, "Name", "ASC", 1, 10, model.CategoryType).FirstOrDefault(x => x.Name == model.Name && x.CategoryId != model.CategoryId);
                if (existingName != null)
                {
                    ModelState.AddModelError("Name", "Tên danh mục này đã tồn tại.");
                }

                if (ModelState.IsValid)
                {
                    model.UpdatedBy = User.Identity?.Name ?? "Admin";
                    SCategory.Update(model);
                    // KHÔNG gọi SSeo.RefreshSitemapAndCache() ở đây - SCategory.Update đã gọi rồi
                    return RedirectToAction("Index", new { type = model.CategoryType });
                }
            }
            ViewBag.Categories = SCategory.GetHierarchical().Where(x => x.CategoryType == model.CategoryType).ToList();
            return View(model);
        }

        [HttpPost]
        [Permission("Categories", ActionType.Delete)]
        public IActionResult Delete(int id)
        {
            try
            {
                SCategory.Delete(id);
                // KHÔNG gọi SSeo.RefreshSitemapAndCache() - SCategory.Delete đã gọi rồi
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [Permission("Categories", ActionType.Delete)]
        public IActionResult BulkDelete([FromBody] List<int> ids)
        {
            try
            {
                // Dùng BulkDelete tập trung: chỉ clear cache + refresh sitemap 1 LẦN
                SCategory.BulkDelete(ids);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [Permission("Categories", ActionType.Edit)]
        public IActionResult UpdateQuick(int id, int? status, int? sortOrder)
        {
            var result = SCategory.UpdateQuick(id, status, sortOrder);
            if (result > 0) {
                // KHÔNG gọi SSeo.RefreshSitemapAndCache() - SCategory.UpdateQuick đã gọi rồi
                return Json(new { success = true });
            }
            return Json(new { success = false, message = "Cập nhật thất bại" });
        }

        [Permission("Categories", ActionType.Export)]
        public IActionResult ExportCSV(string kw = "", int? parent = null, int? status = null)
        {
            // Giới hạn export 10,000 records thay vì 100,000 để tránh OutOfMemory
            var categories = SCategory.Search(kw, parent, status, "SortOrder", "ASC", 1, 10000);
            
            // Ước tính capacity để StringBuilder không phải resize nhiều lần
            var builder = new System.Text.StringBuilder(categories.Count * 100);
            builder.AppendLine("CategoryId,ParentId,Name,Slug,ProductCount,Status,CreatedAt");

            foreach (var item in categories)
            {
                builder.AppendLine($"{item.CategoryId},{item.ParentId},{item.Name.Replace(",", " ")},{item.Slug},{item.ItemCount},{item.Status},{item.CreatedAt:yyyy-MM-dd}");
            }

            // Dùng GetBytes trực tiếp thay vì Concat + ToArray (giảm 2 bản copy RAM)
            var csvBytes = System.Text.Encoding.UTF8.GetBytes(builder.ToString());
            var bom = System.Text.Encoding.UTF8.GetPreamble();
            var result = new byte[bom.Length + csvBytes.Length];
            Buffer.BlockCopy(bom, 0, result, 0, bom.Length);
            Buffer.BlockCopy(csvBytes, 0, result, bom.Length, csvBytes.Length);
            
            return File(result, "text/csv", $"Categories_{DateTime.Now:yyyyMMddHHmm}.csv");
        }

        [HttpPost]
        [Permission("Categories", ActionType.Edit)]
        public IActionResult BulkUpdateStatus([FromBody] BulkStatusModel model)
        {
            if (model.Ids == null || !model.Ids.Any()) return Json(new { success = false });
            
            // Dùng BulkUpdateQuick tập trung: chỉ clear cache 1 LẦN
            var updated = SCategory.BulkUpdateQuick(model.Ids, model.Status, null);
            return Json(new { success = updated > 0 });
        }

        public class BulkStatusModel {
            public List<int> Ids { get; set; } = new();
            public int Status { get; set; }
        }

        [HttpPost]
        [Permission("Categories", ActionType.Edit)]
        public IActionResult ToggleHomeCategory(int id)
        {
            try
            {
                var result = SHomeCategorySetting.Toggle(id);
                return Json(new { success = result > 0 });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [Permission("Categories", ActionType.Edit)]
        public IActionResult UpdateHomeCategory(int id, int sortOrder, int productCount)
        {
            try
            {
                var result = SHomeCategorySetting.Update(id, sortOrder, productCount);
                return Json(new { success = result > 0 });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
