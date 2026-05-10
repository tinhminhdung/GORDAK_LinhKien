using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Antigravity.ECommerce.Models;
using Antigravity.ECommerce.Services;
using Antigravity.ECommerce.Framework;
using System.Collections.Generic;

namespace Antigravity.ECommerce.Controllers
{
    [Authorize]
    [Permission("Menu")]
    public class AdminMenuController : Controller
    {
        public IActionResult Index(string kw = "", int? status = null, string? menuPosition = null, int page = 1, int size = 20)
        {
            // Lấy tất cả menu dạng cây (hierarchical)
            var allMenu = SCategory.GetAll().Where(x => x.CategoryType == 0).ToList();
            
            // Lọc theo từ khoá và trạng thái nếu có
            if (!string.IsNullOrEmpty(kw))
                allMenu = allMenu.Where(x => x.Name.Contains(kw, StringComparison.OrdinalIgnoreCase) || (x.Slug ?? "").Contains(kw, StringComparison.OrdinalIgnoreCase)).ToList();
            if (status.HasValue)
                allMenu = allMenu.Where(x => x.Status == status.Value).ToList();
            if (!string.IsNullOrEmpty(menuPosition))
                allMenu = allMenu.Where(x => !string.IsNullOrEmpty(x.MenuPosition) && x.MenuPosition.Contains(menuPosition, StringComparison.OrdinalIgnoreCase)).ToList();

            // Paginate only ROOT items, children always shown
            var roots = allMenu.Where(x => x.ParentId == 0).OrderBy(x => x.SortOrder).ToList();
            int totalRoots = roots.Count;
            var pagedRoots = roots.Skip((page - 1) * size).Take(size).ToList();
            
            // Dùng SCategory.BuildTree tập trung (clone, an toàn, có giới hạn depth)
            foreach (var r in pagedRoots) 
                r.Children = SCategory.BuildTree(allMenu, r.CategoryId);

            ViewBag.Keyword = kw;
            ViewBag.Status = status;
            ViewBag.MenuPosition = menuPosition;
            ViewBag.TotalCount = allMenu.Count;
            ViewBag.PageIndex = page;
            ViewBag.PageSize = size;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalRoots / size);
            
            // Flat list cho dropdown "Gắn vào Menu cha" trong modal Import
            // Lấy tất cả menu (không filter position) để admin chọn tự do
            ViewBag.Categories = SCategory.GetAll()
                .Where(x => x.CategoryType == 0 && x.Status == 1)
                .OrderBy(x => x.ParentId)
                .ThenBy(x => x.SortOrder)
                .ToList();

            return View(pagedRoots);
        }

        private void PrepareMenuBag()
        {
            // Gọi GetHierarchical() CHỈ 1 LẦN rồi filter (trước đây gọi 7 lần = 7× build đệ quy)
            var allHierarchical = SCategory.GetHierarchical();
            ViewBag.Categories = allHierarchical.Where(x => x.CategoryType == 0).ToList();
            ViewBag.ProductCategories = allHierarchical.Where(x => x.CategoryType == 1).ToList();
            ViewBag.NewsCategories = allHierarchical.Where(x => x.CategoryType == 2).ToList();
            ViewBag.VideoCategories = allHierarchical.Where(x => x.CategoryType == 3).ToList();
            ViewBag.FAQCategories = allHierarchical.Where(x => x.CategoryType == 4).ToList();
            ViewBag.DocumentCategories = allHierarchical.Where(x => x.CategoryType == 5).ToList();
            ViewBag.GalleryCategories = allHierarchical.Where(x => x.CategoryType == 6).ToList();
        }

        public IActionResult Create(int? parentId = null)
        {
            PrepareMenuBag();

            // Kế thừa MenuPosition từ cha — tránh admin phải chọn lại vị trí
            string inheritedPosition = "Header"; // default
            if (parentId.HasValue && parentId.Value > 0)
            {
                var parent = SCategory.GetById(parentId.Value);
                if (parent != null && !string.IsNullOrEmpty(parent.MenuPosition))
                    inheritedPosition = parent.MenuPosition;
            }

            return View(new Category
            {
                SortOrder = 0,
                Status = 1,
                CategoryType = 0,
                ParentId = parentId ?? 0,
                LinkType = 1,
                MenuPosition = inheritedPosition
            });
        }

        [HttpPost]
        public IActionResult Create(Category model, string[] MenuPosition)
        {
            model.CategoryType = 0; // Force Menu Item
            if (ModelState.IsValid)
            {
                // Duplicate Slug Check
                if (!string.IsNullOrEmpty(model.Slug))
                {
                    var existing = Antigravity.ECommerce.Framework.FCategory.GetBySlug(model.Slug);
                    if (existing != null)
                    {
                        ModelState.AddModelError("Slug", "Slug (URL) này đã tồn tại trong hệ thống Menu.");
                    }
                }

                // Duplicate Name Check
                var existingName = SCategory.Search(model.Name, null, null, "Name", "ASC", 1, 10, 0).FirstOrDefault(x => x.Name == model.Name);
                if (existingName != null)
                {
                    ModelState.AddModelError("Name", "Tên Menu này đã tồn tại.");
                }

                if (ModelState.IsValid)
                {
                    if (MenuPosition != null && MenuPosition.Length > 0)
                    {
                        model.MenuPosition = string.Join(",", MenuPosition);
                    }
                    model.CreatedBy = User.Identity?.Name ?? "Admin";
                    SCategory.Insert(model);
                    return RedirectToAction("Index");
                }
            }
            PrepareMenuBag();
            return View(model);
        }

        public IActionResult Edit(int id)
        {
            var item = SCategory.GetById(id);
            if (item == null || item.CategoryType != 0) return NotFound();

            PrepareMenuBag();
            return View(item);
        }

        [HttpPost]
        public IActionResult Edit(Category model, string[] MenuPosition)
        {
            model.CategoryType = 0; // Force Menu Item
            if (ModelState.IsValid)
            {
                // Duplicate Slug Check
                if (!string.IsNullOrEmpty(model.Slug))
                {
                    var existing = Antigravity.ECommerce.Framework.FCategory.GetBySlug(model.Slug);
                    if (existing != null && existing.CategoryId != model.CategoryId)
                    {
                        ModelState.AddModelError("Slug", "Slug (URL) này đã được sử dụng ở Menu khác.");
                    }
                }

                // Duplicate Name Check
                var existingName = SCategory.Search(model.Name, null, null, "Name", "ASC", 1, 10, 0).FirstOrDefault(x => x.Name == model.Name && x.CategoryId != model.CategoryId);
                if (existingName != null)
                {
                    ModelState.AddModelError("Name", "Tên Menu này đã tồn tại.");
                }

                if (ModelState.IsValid)
                {
                    if (MenuPosition != null && MenuPosition.Length > 0)
                    {
                        model.MenuPosition = string.Join(",", MenuPosition);
                    }
                    model.UpdatedBy = User.Identity?.Name ?? "Admin";
                    SCategory.Update(model);
                    return RedirectToAction("Index");
                }
            }
            PrepareMenuBag();
            return View(model);
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            SCategory.Delete(id);
            return Json(new { success = true });
        }

        [HttpPost]
        public IActionResult BulkDelete([FromBody] List<int> ids)
        {
            if (ids == null || ids.Count == 0) return Json(new { success = false });
            // Dùng BulkDelete: chỉ clear cache + refresh sitemap 1 LẦN
            SCategory.BulkDelete(ids);
            return Json(new { success = true });
        }

        [HttpPost]
        public IActionResult BulkUpdateStatus([FromBody] BulkUpdateModel model)
        {
            if (model.Ids == null || model.Ids.Count == 0) return Json(new { success = false });
            // Dùng BulkUpdateQuick: chỉ clear cache 1 LẦN thay vì N lần
            var updated = SCategory.BulkUpdateQuick(model.Ids, model.Status, null);
            return Json(new { success = updated > 0 });
        }

        [HttpPost]
        public IActionResult UpdateQuick(int id, int? status, int? sortOrder)
        {
            var result = SCategory.UpdateQuick(id, status, sortOrder);
            if (result > 0) return Json(new { success = true });
            return Json(new { success = false });
        }

        public class BulkUpdateModel
        {
            public List<int> Ids { get; set; } = new();
            public int Status { get; set; }
        }

        [HttpGet]
        public IActionResult GetModuleCategories(int module)
        {
            // 1: Product, 2: News, 3: Video, 4: FAQ, 5: Document, 6: Gallery
            var categories = SCategory.GetHierarchical().Where(x => x.CategoryType == module).ToList();
            var allMenus = SCategory.GetAll().Where(x => x.CategoryType == 0).ToList();

            // Lấy tất cả URL của menu hiện tại để so sánh
            var menuUrls = allMenus.Where(x => !string.IsNullOrEmpty(x.Url)).Select(x => x.Url).ToHashSet();

            string GenerateUrl(string slug, int type)
            {
                if (type == 1) return "/san-pham/" + slug + ".html";
                if (type == 2) return "/tin-tuc/" + slug + ".html";
                if (type == 3) return "/video/" + slug + ".html";
                if (type == 4) return "/hoi-dap/" + slug + ".html";
                if (type == 5) return "/tai-lieu/" + slug + ".html";
                if (type == 6) return "/thu-vien-anh/" + slug + ".html";
                return "";
            }

            var result = new List<object>();
            void Flatten(List<Category> list, int level)
            {
                foreach (var item in list)
                {
                    string expectedUrl = GenerateUrl(item.Slug ?? "", module);
                    bool exists = menuUrls.Contains(expectedUrl);
                    result.Add(new {
                        id = item.CategoryId,
                        name = item.Name,
                        level = level,
                        exists = exists
                    });
                    if (item.Children != null && item.Children.Any())
                    {
                        Flatten(item.Children, level + 1);
                    }
                }
            }
            Flatten(categories, 0);

            return Json(result);
        }

        public class BulkImportModel
        {
            public List<int> CategoryIds { get; set; } = new();
            public int ParentId { get; set; }
            public string Position { get; set; } = "Header";
            public int ModuleType { get; set; }
        }

        [HttpPost]
        public IActionResult BulkImport([FromBody] BulkImportModel model)
        {
            if (model.CategoryIds == null || model.CategoryIds.Count == 0) return Json(new { success = false, message = "Chưa chọn danh mục" });

            var allSourceCats = SCategory.GetAll().Where(x => x.CategoryType == model.ModuleType).ToList();
            var selectedCats = allSourceCats.Where(x => model.CategoryIds.Contains(x.CategoryId)).OrderBy(x => x.ParentId).ThenBy(x => x.SortOrder).ToList();

            string GenerateUrl(string slug, int type)
            {
                if (type == 1) return "/san-pham/" + slug + ".html";
                if (type == 2) return "/tin-tuc/" + slug + ".html";
                if (type == 3) return "/video/" + slug + ".html";
                if (type == 4) return "/hoi-dap/" + slug + ".html";
                if (type == 5) return "/tai-lieu/" + slug + ".html";
                if (type == 6) return "/thu-vien-anh/" + slug + ".html";
                return "";
            }

            // Lấy menu hiện tại để kiểm tra trùng
            var allMenus = SCategory.GetAll().Where(x => x.CategoryType == 0).ToList();
            var menuUrls = allMenus.Where(x => !string.IsNullOrEmpty(x.Url)).Select(x => x.Url).ToHashSet();

            // Mapping ID nguồn -> ID menu mới (để giữ cấu trúc cha con)
            var idMapping = new Dictionary<int, int>();
            idMapping[0] = model.ParentId; // Root trỏ về ParentId đã chọn

            int imported = 0;

            foreach (var src in selectedCats)
            {
                string expectedUrl = GenerateUrl(src.Slug ?? "", model.ModuleType);

                // Xác định ParentId mới
                int newParentId = model.ParentId;
                if (idMapping.ContainsKey(src.ParentId))
                {
                    newParentId = idMapping[src.ParentId];
                }

                // Chỉ bỏ qua nếu ĐÃ TỒN TẠI ở CÙNG VỊ TRÍ và CÙNG MENU CHA
                var existingMenu = allMenus.FirstOrDefault(x => x.Url == expectedUrl && x.ParentId == newParentId && x.MenuPosition == model.Position);
                if (existingMenu != null)
                {
                    // Đã tồn tại đúng chỗ này -> map ID để menu con gắn vào, không tạo thêm
                    idMapping[src.CategoryId] = existingMenu.CategoryId;
                    continue;
                }

                var newMenu = new Category
                {
                    CategoryType = 0, // Menu
                    LinkType = 1, // System Link
                    ParentId = newParentId,
                    Name = src.Name,
                    Slug = src.Slug + "-menu-" + DateTime.Now.Ticks.ToString().Substring(12), // Tránh trùng slug
                    Url = expectedUrl,
                    Target = "_self",
                    MenuPosition = model.Position,
                    SortOrder = src.SortOrder,
                    Status = 1,
                    SeoTitle = string.IsNullOrEmpty(src.SeoTitle) ? src.Name : src.SeoTitle,
                    SeoDescription = src.SeoDescription,
                    SeoKeywords = src.SeoKeywords,
                    CreatedBy = User.Identity?.Name ?? "Admin"
                };

                int newId = SCategory.Insert(newMenu);
                if (newId > 0)
                {
                    idMapping[src.CategoryId] = newId;
                    imported++;
                }
            }

            return Json(new { success = true, importedCount = imported });
        }
    }
}
