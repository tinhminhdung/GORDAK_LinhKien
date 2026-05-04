using Microsoft.AspNetCore.Mvc;
using Antigravity.ECommerce.Services;
using Antigravity.ECommerce.Models;
using System.Linq;

namespace Antigravity.ECommerce.Controllers
{
    public class GalleryController : Controller
    {
        #region 1. Trang danh sách Thư viện ảnh
        /// <summary>
        /// Trang danh sách các Album ảnh (Hỗ trợ lọc theo danh mục)
        /// </summary>
        public IActionResult Index(string urlCategory = "", int page = 1)
        {
            if (page < 1) page = 1;
            int pageSize = 12;
            int? categoryId = null;
            
            // Lấy danh sách các danh mục Thư viện ảnh (CategoryType = 6) để hiển thị Menu/Sidebar
            var categories = SCategory.Search(null, null, 1, "SortOrder", "ASC", 1, 100, 6);
            ViewBag.Categories = categories;
            
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
                else
                {
                    ViewData["Title"] = "Thư viện ảnh - Album Gallery";
                }
            }
            else
            {
                ViewData["Title"] = "Thư viện ảnh - Album Gallery";
            }

            // Deferred execution giúp tiết kiệm RAM khi duyệt hàng ngàn Album
            IEnumerable<Gallery> allGalleries = SGallery.GetAll().Where(x => x.Status == 1);
            if (categoryId.HasValue)
            {
                var allCatIds = SCategory.GetDescendantIds(categoryId.Value);
                allGalleries = allGalleries.Where(x => allCatIds.Contains(x.CategoryId));
            }

            var galleries = allGalleries.OrderByDescending(x => x.CreatedAt);
            
            // Tính toán tổng số lượng để phân trang
            int totalCount = galleries.Count();
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalCount = totalCount;
            
            var paginatedData = galleries.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            
            return View(paginatedData);
        }
        #endregion

        #region 2. Trang chi tiết Thư viện ảnh
        /// <summary>
        /// Trang xem chi tiết các ảnh bên trong một Album
        /// </summary>
        public IActionResult Detail(string slug)
        {
            var gallery = SGallery.GetBySlug(slug);
            if (gallery == null || gallery.Status == 0) return NotFound();

            // Lấy ngẫu nhiên 6 Album liên quan cùng danh mục
            ViewBag.Related = SGallery.Search(null, 1, gallery.CategoryId, "CreatedAt", "DESC", 1, 6)
                .Where(g => g.GalleryId != gallery.GalleryId).ToList();

            // Cấu hình thẻ meta SEO
            ViewData["Title"] = !string.IsNullOrEmpty(gallery.SeoTitle) ? gallery.SeoTitle : gallery.AlbumName;
            ViewData["Description"] = gallery.SeoDescription;
            ViewData["Keywords"] = gallery.SeoKeywords;
            if (!string.IsNullOrEmpty(gallery.CoverImage)) ViewData["Image"] = gallery.CoverImage;

            return View(gallery);
        }
        #endregion
    }
}


