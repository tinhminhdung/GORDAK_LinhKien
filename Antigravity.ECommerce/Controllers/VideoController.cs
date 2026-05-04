using Microsoft.AspNetCore.Mvc;
using Antigravity.ECommerce.Services;
using Antigravity.ECommerce.Models;
using System.Linq;

namespace Antigravity.ECommerce.Controllers
{
    public class VideoController : Controller
    {
        #region 1. Trang danh sách Video
        /// <summary>
        /// Trang danh sách toàn bộ Video (Hỗ trợ lọc theo danh mục)
        /// </summary>
        public IActionResult Index(string urlCategory = "", int page = 1)
        {
            if (page < 1) page = 1;
            int pageSize = 9;
            int? categoryId = null;
            
            // Lấy danh sách các danh mục Video (CategoryType = 3) để hiển thị Menu/Sidebar
            var categories = SCategory.Search(null, null, 1, "SortOrder", "ASC", 1, 100, 3);
            ViewBag.Categories = categories;
            
            // Nếu có tham số urlCategory, tìm danh mục tương ứng
            if (!string.IsNullOrEmpty(urlCategory))
            {
                var cat = categories.FirstOrDefault(c => c.Slug == urlCategory);
                if (cat != null)
                {
                    categoryId = cat.CategoryId;
                    ViewBag.CurrentCategory = cat;
                    // Kế thừa cấu hình SEO từ danh mục
                    ViewData["Title"] = !string.IsNullOrEmpty(cat.SeoTitle) ? cat.SeoTitle : cat.Name;
                    ViewData["Description"] = cat.SeoDescription;
                    ViewData["Keywords"] = cat.SeoKeywords;
                }
            }

            if (ViewData["Title"] == null)
            {
                ViewData["Title"] = "Video - Clip nổi bật";
            }

            // Deferred execution giúp tiết kiệm RAM khi duyệt hàng ngàn Video
            IEnumerable<Video> allVideos = SVideo.GetAll().Where(x => x.Status == 1);
            if (categoryId.HasValue)
            {
                var allCatIds = SCategory.GetDescendantIds(categoryId.Value);
                allVideos = allVideos.Where(x => allCatIds.Contains(x.CategoryId));
            }

            var videos = allVideos.OrderByDescending(x => x.CreatedAt);
            
            // Tính toán tổng số lượng để phân trang
            int totalCount = videos.Count();
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalCount = totalCount;
            
            // Trích xuất đúng số lượng video của trang hiện tại
            var paginatedData = videos.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            
            return View(paginatedData);
        }
        #endregion

        #region 2. Trang chi tiết Video
        /// <summary>
        /// Trang xem chi tiết một Video (Phát qua iframe Youtube)
        /// </summary>
        public IActionResult Detail(string slug)
        {
            var video = SVideo.GetBySlug(slug);
            if (video == null || video.Status == 0) return NotFound();

            // Lấy ngẫu nhiên 6 video liên quan cùng danh mục
            ViewBag.Related = SVideo.Search(null, 1, video.CategoryId, "CreatedAt", "DESC", 1, 6)
                .Where(v => v.VideoId != video.VideoId).ToList();

            // Cấu hình thẻ meta SEO
            ViewData["Title"] = !string.IsNullOrEmpty(video.SeoTitle) ? video.SeoTitle : video.Title;
            ViewData["Description"] = video.SeoDescription;
            ViewData["Keywords"] = video.SeoKeywords;
            if (!string.IsNullOrEmpty(video.ThumbnailUrl)) ViewData["Image"] = video.ThumbnailUrl;

            return View(video);
        }
        #endregion
    }
}


