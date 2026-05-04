using Microsoft.AspNetCore.Mvc;
using Antigravity.ECommerce.Services;
using Antigravity.ECommerce.Models;
using System.Linq;

namespace Antigravity.ECommerce.Controllers
{
    public class DocumentController : Controller
    {
        #region 1. Trang danh sách Tài liệu (Download)
        /// <summary>
        /// Trang danh sách các tài liệu (Hỗ trợ lọc theo Danh mục)
        /// </summary>
        public IActionResult Index(string urlCategory = "", int page = 1)
        {
            if (page < 1) page = 1;
            int pageSize = 15;
            int? categoryId = null;
            
            // Lấy danh sách các Danh mục Tài liệu (CategoryType = 8) để hiển thị Menu lọc
            var categories = SCategory.Search(null, null, 1, "SortOrder", "ASC", 1, 100, 8);
            ViewBag.Categories = categories;
            
            // Lọc theo danh mục nếu người dùng click vào
            if (!string.IsNullOrEmpty(urlCategory))
            {
                var cat = categories.FirstOrDefault(c => c.Slug == urlCategory);
                if (cat != null)
                {
                    categoryId = cat.CategoryId;
                    ViewBag.CurrentCategory = cat;
                    // Kế thừa SEO từ danh mục
                    ViewData["Title"] = !string.IsNullOrEmpty(cat.SeoTitle) ? cat.SeoTitle : cat.Name;
                    ViewData["Description"] = cat.SeoDescription;
                    ViewData["Keywords"] = cat.SeoKeywords;
                }
                else
                {
                    ViewData["Title"] = "Tài liệu - Download Documents";
                }
            }
            else
            {
                ViewData["Title"] = "Tài liệu - Download Documents";
            }

            // Sử dụng IEnumerable (Deferred execution) để tối ưu hóa bộ nhớ
            IEnumerable<Document> allDocs = SDocument.GetAll().Where(x => x.Status == 1);
            if (categoryId.HasValue)
            {
                var allCatIds = SCategory.GetDescendantIds(categoryId.Value);
                allDocs = allDocs.Where(x => allCatIds.Contains(x.CategoryId));
            }

            // Tài liệu thường được hiển thị theo thứ tự ưu tiên (SortOrder)
            var documents = allDocs.OrderBy(x => x.SortOrder);
            
            int totalCount = documents.Count();
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalCount = totalCount;
            
            var paginatedData = documents.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            
            return View(paginatedData);
        }
        #endregion
    }
}
