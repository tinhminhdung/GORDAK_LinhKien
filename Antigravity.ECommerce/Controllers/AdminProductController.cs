using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Antigravity.ECommerce.Models;
using Antigravity.ECommerce.Services;
using Antigravity.ECommerce.Framework;

namespace Antigravity.ECommerce.Controllers
{
    [Authorize]
    [Permission("Products", ActionType.View)]
    public class AdminProductController : Controller
    {
        public IActionResult Index(string kw = "", string[]? categories = null, int? status = null, bool? hot = null, 
            decimal? priceMin = null, decimal? priceMax = null,
            DateTime? dateMin = null, DateTime? dateMax = null,
            string sort = "CreatedAt", string order = "DESC", int page = 1, int size = 20)
        {
            string? catIds = categories != null && categories.Length > 0 ? string.Join(",", categories) : null;
            var products = SProduct.Search(kw, catIds, status, hot, priceMin, priceMax, sort, order, page, size, dateMin, dateMax);
            
            ViewBag.Keyword = kw;
            ViewBag.CategoryIds = catIds;
            ViewBag.Status = status;
            ViewBag.IsHot = hot;
            ViewBag.PriceMin = priceMin;
            ViewBag.PriceMax = priceMax;
            ViewBag.DateMin = dateMin?.ToString("yyyy-MM-dd");
            ViewBag.DateMax = dateMax?.ToString("yyyy-MM-dd");
            ViewBag.SortColumn = sort;
            ViewBag.SortOrder = order;
            ViewBag.PageIndex = page;
            ViewBag.PageSize = size;
            ViewBag.TotalCount = products.Count > 0 ? products[0].TotalCount : 0;
            
            // Only show Product Categories (Type 1)
            ViewBag.Categories = SCategory.GetHierarchical().Where(x => x.CategoryType == 1).ToList();
            return View(products);
        }


        [Permission("Products", ActionType.Create)]
        public IActionResult Create()
        {
            ViewBag.Categories = SCategory.GetHierarchical().Where(x => x.CategoryType == 1).ToList();
            ViewBag.Brands = SProductOption.GetAll().Where(x => x.Type == 1).ToList();
            ViewBag.Conditions = SProductOption.GetAll().Where(x => x.Type == 2).ToList();
            ViewBag.Warranties = SProductOption.GetAll().Where(x => x.Type == 3).ToList();
            ViewBag.AllProducts = SProduct.GetAll(); // Để chọn sản phẩm liên quan
            return View(new Product { Status = 1, Price = 0, Stock = 0, OldPrice = 0, PurchasePrice = 0 });
        }

        [HttpPost]
        [Permission("Products", ActionType.Create)]
        public IActionResult Create(Product model)
        {
            if (ModelState.IsValid)
            {
                var existing = FProduct.GetBySlug(model.Slug);
                if (existing != null)
                {
                    ModelState.AddModelError("Slug", "Slug (URL chia sẻ) này đã tồn tại, vui lòng nhập slug khác để không bị trùng lặp đường dẫn.");
                }
                else
                {
                    model.CreatedBy = User.Identity?.Name ?? "Admin";
                    SProduct.Insert(model);
                    SSeo.RefreshSitemapAndCache();
                    return RedirectToAction("Index");
                }
            }
            ViewBag.Categories = SCategory.GetHierarchical().Where(x => x.CategoryType == 1).ToList();
            ViewBag.Brands = SProductOption.GetAll().Where(x => x.Type == 1).ToList();
            ViewBag.Conditions = SProductOption.GetAll().Where(x => x.Type == 2).ToList();
            ViewBag.Warranties = SProductOption.GetAll().Where(x => x.Type == 3).ToList();
            ViewBag.AllProducts = SProduct.GetAll();
            return View(model);
        }


        [Permission("Products", ActionType.Edit)]
        public IActionResult Edit(int id)
        {
            var product = SProduct.GetById(id);
            if (product == null) return NotFound();
            
            ViewBag.Categories = SCategory.GetHierarchical().Where(x => x.CategoryType == 1).ToList();
            ViewBag.Brands = SProductOption.GetAll().Where(x => x.Type == 1).ToList();
            ViewBag.Conditions = SProductOption.GetAll().Where(x => x.Type == 2).ToList();
            ViewBag.Warranties = SProductOption.GetAll().Where(x => x.Type == 3).ToList();
            ViewBag.AllProducts = SProduct.GetAll(); // Để chọn sản phẩm liên quan
            return View(product);
        }

        [HttpPost]
        [Permission("Products", ActionType.Edit)]
        public IActionResult Edit(Product model)
        {
            if (ModelState.IsValid)
            {
                var existing = FProduct.GetBySlug(model.Slug);
                if (existing != null && existing.ProductId != model.ProductId)
                {
                    ModelState.AddModelError("Slug", "Slug (URL chia sẻ) này đã tồn tại ở sản phẩm khác, vui lòng nhập slug khác để không bị trùng lặp đường dẫn.");
                }
                else
                {
                    model.UpdatedBy = User.Identity?.Name ?? "Admin";
                    SProduct.Update(model);
                    SSeo.RefreshSitemapAndCache();
                    return RedirectToAction("Index");
                }
            }
            ViewBag.Categories = SCategory.GetHierarchical().Where(x => x.CategoryType == 1).ToList();
            ViewBag.Brands = SProductOption.GetAll().Where(x => x.Type == 1).ToList();
            ViewBag.Conditions = SProductOption.GetAll().Where(x => x.Type == 2).ToList();
            ViewBag.Warranties = SProductOption.GetAll().Where(x => x.Type == 3).ToList();
            ViewBag.AllProducts = SProduct.GetAll();
            return View(model);
        }

        [HttpPost]
        [Permission("Products", ActionType.Delete)]
        public IActionResult Delete(int id)
        {
            try
            {
                SProduct.Delete(id);
                SSeo.RefreshSitemapAndCache();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("REFERENCE constraint") || ex.Message.Contains("FK_"))
                {
                    return Json(new { success = false, type = "RESTRICED", message = "Không thể xóa sản phẩm này vì đã có trong dữ liệu đơn hàng. Vui lòng chuyển trạng thái sang 'Ẩn' hoặc xóa các đơn hàng liên quan trước khi xóa sản phẩm." });
                }
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [Permission("Products", ActionType.Delete)]
        public IActionResult ForceDelete(int id)
        {
            try
            {
                SProduct.ForceDelete(id);
                SSeo.RefreshSitemapAndCache();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [Permission("Products", ActionType.Delete)]
        public IActionResult BulkDelete([FromBody] List<int> ids)
        {
            try
            {
                if (ids == null || !ids.Any()) return Json(new { success = false, message = "Chưa chọn sản phẩm" });

                int successCount = 0;
                int failCount = 0;
                string lastError = "";

                foreach (var id in ids)
                {
                    try {
                        SProduct.Delete(id);
                        successCount++;
                    } catch (Exception ex) {
                        failCount++;
                        if (ex.Message.Contains("REFERENCE constraint") || ex.Message.Contains("FK_")) {
                            lastError = "Một số sản phẩm không thể xóa do đã có trong đơn hàng.";
                        } else {
                            lastError = ex.Message;
                        }
                    }
                }

                if (failCount > 0)
                {
                    return Json(new { success = true, message = $"Đã xóa {successCount} sản phẩm. {failCount} sản phẩm không thể xóa. {lastError}" });
                }

                SSeo.RefreshSitemapAndCache();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [Permission("Products", ActionType.Edit)]
        public IActionResult BulkUpdateStatus([FromBody] BulkUpdateModel model)
        {
            try
            {
                foreach (var id in model.Ids)
                {
                    SProduct.UpdateQuick(id, model.Status, model.IsHot);
                }
                SSeo.RefreshSitemapAndCache();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [Permission("Products", ActionType.Export)]
        public IActionResult ExportCSV(string kw = "", string[]? categories = null, int? status = null, bool? hot = null, 
            decimal? priceMin = null, decimal? priceMax = null,
            DateTime? dateMin = null, DateTime? dateMax = null)
        {
            string? catIds = categories != null && categories.Length > 0 ? string.Join(",", categories) : null;
            var products = SProduct.Search(kw, catIds, status, hot, priceMin, priceMax, "CreatedAt", "DESC", 1, 1000000, dateMin, dateMax);

            var builder = new System.Text.StringBuilder();
            builder.AppendLine("ProductId,SKU,Name,Price,Stock,Status,CreatedAt");

            foreach (var item in products)
            {
                builder.AppendLine($"{item.ProductId},{item.SKU},{item.Name.Replace(",", " ")},{item.Price},{item.Stock},{item.Status},{item.CreatedAt:yyyy-MM-dd}");
            }

            return File(System.Text.Encoding.UTF8.GetPreamble().Concat(System.Text.Encoding.UTF8.GetBytes(builder.ToString())).ToArray(), "text/csv", $"Products_{DateTime.Now:yyyyMMddHHmm}.csv");
        }

        public class BulkUpdateModel {
            public List<int> Ids { get; set; } = new();
            public int? Status { get; set; }
            public bool? IsHot { get; set; }
        }

        [HttpPost]
        [Permission("Products", ActionType.Edit)]
        public IActionResult UpdateQuick(int id, int? status, bool? hot)
        {
            var result = SProduct.UpdateQuick(id, status, hot);
            if (result > 0) {
                SSeo.RefreshSitemapAndCache();
                return Json(new { success = true });
            }
            return Json(new { success = false, message = "Cập nhật thất bại" });
        }

        [HttpPost]
        [Permission("Products", ActionType.Create)]
        public IActionResult Duplicate(int id)
        {
            try
            {
                var original = SProduct.GetById(id);
                if (original == null) return Json(new { success = false, message = "Không tìm thấy sản phẩm gốc" });

                var clone = original;
                clone.Name = "Copy of " + original.Name;
                clone.Slug = original.Slug + "-copy-" + DateTime.Now.Ticks;
                clone.SKU = original.SKU + "-COPY";
                clone.CreatedAt = DateTime.Now;
                clone.CreatedBy = "Admin";
                clone.Status = 0; // Clone should be hidden initially

                int newId = SProduct.Insert(clone);
                SSeo.RefreshSitemapAndCache();
                return Json(new { success = true, newId = newId });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
