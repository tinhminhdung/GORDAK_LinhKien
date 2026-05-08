using Microsoft.AspNetCore.Mvc;
using Antigravity.ECommerce.Services;
using Antigravity.ECommerce.Models;
using System.Linq;
using Antigravity.ECommerce.Framework;

namespace Antigravity.ECommerce.Controllers
{
    public class NewsController : Controller
    {
        #region 1. Trang danh sách Tin tức
        /// <summary>
        /// Trang danh sách toàn bộ tin tức (Hỗ trợ tìm kiếm từ khóa)
        /// </summary>
        public IActionResult Index(string kw = "", int page = 1)
        {
            var settings = SSetting.GetViewModel();
            int pageSize = settings.NewsPageSize > 0 ? settings.NewsPageSize : 12;
            int bigCount = settings.NewsBigCount > 0 ? settings.NewsBigCount : 2;

            var allNews = SNews.GetAll().Where(x => x.Status == 1).ToList();

            // Nếu không có tìm kiếm và đang ở trang 1, lấy tin NỔI BẬT riêng
            List<News> bigNews = new List<News>();
            if (string.IsNullOrEmpty(kw) && page == 1)
            {
                bigNews = allNews.Where(x => x.IsHot).OrderByDescending(x => x.SortOrder).ThenByDescending(x => x.CreatedAt).Take(bigCount).ToList();
                
                // Loại trừ các bài đã nằm trong BigNews khỏi danh sách nhỏ để tránh trùng lặp
                var bigIds = bigNews.Select(x => x.NewsId).ToList();
                allNews = allNews.Where(x => !bigIds.Contains(x.NewsId)).ToList();
            }

            IEnumerable<News> query = allNews;
            
            // Lọc theo từ khóa (nếu có)
            if (!string.IsNullOrEmpty(kw))
            {
                query = query.Where(x => x.Title.Contains(kw, System.StringComparison.OrdinalIgnoreCase) 
                                 || (x.Tags != null && x.Tags.Contains(kw, System.StringComparison.OrdinalIgnoreCase)));
            }

            var data = query.OrderByDescending(x => x.CreatedAt);
            
            int total = data.Count();
            var paginatedData = data.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            
            ViewBag.Keyword = kw;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)System.Math.Ceiling((double)total / pageSize);
            ViewBag.BigNews = bigNews;

            // Bổ sung dữ liệu cho Sidebar & Filter
            ViewBag.Categories = SCategory.GetAll().Where(x => x.Status == 1 && x.CategoryType == 2).OrderBy(x => x.SortOrder).ToList();
            ViewBag.HotNews = SNews.GetAll().Where(x => x.Status == 1 && x.IsHot).OrderByDescending(x => x.SortOrder).ThenByDescending(x => x.CreatedAt).Take(5).ToList();
            ViewBag.TotalNewsCount = SNews.GetAll().Count(x => x.Status == 1);

            // Cấu hình thẻ meta SEO
            ViewData["Title"] = string.IsNullOrEmpty(kw) ? "Tin tức - Bài viết mới nhất" : $"Tìm kiếm: {kw} - Tin tức";
            ViewData["Description"] = "Cập nhật tin tức mới nhất, bài viết chuyên sâu và thông tin hữu ích.";
            
            return View(paginatedData);
        }

        /// <summary>
        /// Trang danh sách tin tức theo Danh mục cụ thể
        /// </summary>
        public IActionResult Category(string slug, int page = 1)
        {
            var category = FCategory.GetBySlug(slug);
            // Kểm tra tồn tại và đúng loại danh mục Tin tức (CategoryType = 2)
            if (category == null || category.CategoryType != 2) return NotFound();

            // Đệ quy lấy tất cả ID của danh mục con cháu để lọc bài viết
            var allCatIds = SCategory.GetDescendantIds(category.CategoryId);
            var query = SNews.GetAll().Where(x => x.Status == 1 && allCatIds.Contains(x.CategoryId)).OrderByDescending(x => x.CreatedAt);
            
            int pageSize = 12;
            int total = query.Count();
            var paginatedData = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            
            ViewBag.Category = category;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)System.Math.Ceiling((double)total / pageSize);

            // Bổ sung dữ liệu cho Sidebar & Filter
            ViewBag.Categories = SCategory.GetAll().Where(x => x.Status == 1 && x.CategoryType == 2).OrderBy(x => x.SortOrder).ToList();
            ViewBag.HotNews = SNews.GetAll().Where(x => x.Status == 1 && x.IsHot).OrderByDescending(x => x.SortOrder).ThenByDescending(x => x.CreatedAt).Take(5).ToList();
            ViewBag.TotalNewsCount = SNews.GetAll().Count(x => x.Status == 1);

            // Cấu hình thẻ meta SEO
            ViewData["Title"] = !string.IsNullOrEmpty(category.SeoTitle) ? category.SeoTitle : category.Name;
            ViewData["Description"] = category.SeoDescription;
            ViewData["Keywords"] = category.SeoKeywords;
            
            // Dùng chung giao diện với Index
            return View("Index", paginatedData);
        }
        #endregion

        #region 2. Trang chi tiết Bài viết
        /// <summary>
        /// Trang đọc chi tiết nội dung Bài viết
        /// </summary>
        public IActionResult Detail(string slug)
        {
            var item = SNews.GetAll().FirstOrDefault(x => x.Slug == slug && x.Status == 1);
            if (item == null) return Redirect("/");
            
            // Cập nhật lượt xem trực tiếp vào Database (không chờ Cache)
            Antigravity.ECommerce.Services.BaseConnectionSql.ExecuteNonQuery("UPDATE News SET Views = Views + 1 WHERE NewsId = " + item.NewsId);
            item.Views += 1;
            
            // Lấy ngẫu nhiên 5 bài viết liên quan (cùng trạng thái)
            ViewBag.RelatedNews = SNews.GetAll().Where(x => x.Status == 1 && x.NewsId != item.NewsId).OrderByDescending(x => x.CreatedAt).Take(5).ToList();

            // Bổ sung dữ liệu cho Sidebar
            ViewBag.Categories = SCategory.GetAll().Where(x => x.Status == 1 && x.CategoryType == 2).OrderBy(x => x.SortOrder).ToList();
            ViewBag.HotNews = SNews.GetAll().Where(x => x.Status == 1 && x.IsHot).OrderByDescending(x => x.SortOrder).ThenByDescending(x => x.CreatedAt).Take(5).ToList();
            ViewBag.TotalNewsCount = SNews.GetAll().Count(x => x.Status == 1);

            // Cấu hình thẻ meta SEO
            ViewData["Title"] = !string.IsNullOrEmpty(item.SeoTitle) ? item.SeoTitle : item.Title;
            ViewData["Description"] = !string.IsNullOrEmpty(item.SeoDescription) ? item.SeoDescription : item.ShortDescription;
            ViewData["Keywords"] = item.SeoKeywords;
            if (!string.IsNullOrEmpty(item.Image)) ViewData["Image"] = item.Image;
            
            return View(item);
        }
        #endregion
    }
}
