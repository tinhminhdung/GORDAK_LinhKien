using Microsoft.AspNetCore.Mvc;
using Antigravity.ECommerce.Services;
using Antigravity.ECommerce.Models;
using System.Linq;

namespace Antigravity.ECommerce.Controllers
{
    public class FAQController : Controller
    {
        #region 1. Trang danh sách Hỏi đáp (FAQ)
        /// <summary>
        /// Trang danh sách các câu hỏi thường gặp (Hỗ trợ lọc theo Chủ đề/Danh mục)
        /// </summary>
        public IActionResult Index(string urlCategory = "", int page = 1)
        {
            if (page < 1) page = 1;
            int pageSize = 20;
            int? categoryId = null;
            
            // Lấy danh sách các Chủ đề FAQ (CategoryType = 7) để hiển thị Menu lọc
            var categories = SCategory.Search(null, null, 1, "SortOrder", "ASC", 1, 100, 7);
            ViewBag.Categories = categories;
            
            // Lọc theo chủ đề nếu người dùng click vào
            if (!string.IsNullOrEmpty(urlCategory))
            {
                var cat = categories.FirstOrDefault(c => c.Slug == urlCategory);
                if (cat != null)
                {
                    categoryId = cat.CategoryId;
                    ViewBag.CurrentCategory = cat;
                    // Kế thừa SEO từ chủ đề
                    ViewData["Title"] = !string.IsNullOrEmpty(cat.SeoTitle) ? cat.SeoTitle : cat.Name;
                    ViewData["Description"] = cat.SeoDescription;
                    ViewData["Keywords"] = cat.SeoKeywords;
                }
                else
                {
                    ViewData["Title"] = "Câu hỏi thường gặp - FAQ";
                }
            }
            else
            {
                ViewData["Title"] = "Câu hỏi thường gặp - FAQ";
            }

            // Sử dụng IEnumerable (Deferred execution) để tối ưu hóa bộ nhớ
            IEnumerable<FAQ> allFaqs = SFAQ.GetAll().Where(x => x.Status == 1);
            if (categoryId.HasValue)
            {
                var allCatIds = SCategory.GetDescendantIds(categoryId.Value);
                allFaqs = allFaqs.Where(x => allCatIds.Contains(x.CategoryId));
            }

            // FAQ thường được hiển thị theo thứ tự do Admin sắp xếp (SortOrder) thay vì mới nhất
            var faqs = allFaqs.OrderBy(x => x.SortOrder);
            
            int totalCount = faqs.Count();
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalCount = totalCount;
            
            var paginatedData = faqs.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            
            return View(paginatedData);
        }
        #endregion
    }
}
