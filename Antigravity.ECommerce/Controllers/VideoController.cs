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
            
            // Tổng số video để hiển thị cạnh "Tất cả"
            var allActive = SVideo.GetAll().Where(x => x.Status == 1).ToList();
            ViewBag.TotalVideoCount = allActive.Count;
            
            // Nếu có tham số urlCategory, tìm danh mục tương ứng
            if (!string.IsNullOrEmpty(urlCategory))
            {
                var cat = categories.FirstOrDefault(c => c.Slug == urlCategory);
                if (cat != null)
                {
                    categoryId = cat.CategoryId;
                    ViewBag.CurrentCategory = cat;
                    ViewData["Title"] = !string.IsNullOrEmpty(cat.SeoTitle) ? cat.SeoTitle : cat.Name;
                    ViewData["Description"] = cat.SeoDescription;
                    ViewData["Keywords"] = cat.SeoKeywords;
                }
            }

            if (ViewData["Title"] == null)
            {
                ViewData["Title"] = "Video - Clip nổi bật";
            }

            var allVideosList = allActive;
            if (categoryId.HasValue)
            {
                var allCatIds = SCategory.GetDescendantIds(categoryId.Value);
                allVideosList = allVideosList.Where(x => allCatIds.Contains(x.CategoryId)).ToList();
            }

            var sortedVideos = allVideosList.OrderByDescending(x => x.SortOrder).ThenByDescending(x => x.CreatedAt).ToList();
            
            // Tách video đầu tiên (luôn hiện trên cùng) và phân trang phần còn lại
            Video? featuredVideo = sortedVideos.FirstOrDefault();
            var remainingVideos = sortedVideos.Skip(1).ToList();
            
            int totalCount = remainingVideos.Count;
            ViewBag.FeaturedVideo = featuredVideo;
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalCount = totalCount;
            
            // Video nổi bật cho Sidebar - chỉ lấy video có IsHot = true
            ViewBag.HotVideos = allActive.Where(x => x.IsHot).OrderByDescending(x => x.SortOrder).ThenByDescending(x => x.CreatedAt).Take(5).ToList();
            
            // Phân trang chỉ áp dụng cho phần video nhỏ bên dưới
            var paginatedData = remainingVideos.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            
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

            // Tăng lượt xem
            SSeo.IncrementViewCount("Videos", "VideoId", video.VideoId);
            video.Views += 1;

            // Lấy ngẫu nhiên 6 video liên quan cùng danh mục
            ViewBag.Related = SVideo.Search(null, 1, video.CategoryId, "CreatedAt", "DESC", 1, 6)
                .Where(v => v.VideoId != video.VideoId).ToList();

            // Bổ sung dữ liệu cho Sidebar
            var categories = SCategory.Search(null, null, 1, "SortOrder", "ASC", 1, 100, 3);
            ViewBag.Categories = categories;
            var allActive = SVideo.GetAll().Where(x => x.Status == 1).ToList();
            ViewBag.TotalVideoCount = allActive.Count;
            ViewBag.HotVideos = allActive.Where(x => x.IsHot).OrderByDescending(x => x.SortOrder).ThenByDescending(x => x.CreatedAt).Take(5).ToList();

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


