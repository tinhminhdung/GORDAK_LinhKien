using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Antigravity.ECommerce.Models;

namespace Antigravity.ECommerce.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    #region 1. Trang Chủ
    /// <summary>
    /// Trang chủ mặc định của website, tải tổng hợp các khối dữ liệu hiển thị (Đã có Cache 60 phút toàn bộ)
    /// </summary>
    public IActionResult Index()
    {
        // 1. Lấy danh sách Sản phẩm Nổi bật
        var hotProducts = Antigravity.ECommerce.Services.SCache.GetOrSet("Home_HotProducts", () => 
            Antigravity.ECommerce.Framework.FProduct.Search(null, null, 1, true, null, null, "CreatedAt", "DESC", 1, 8), 60);
            
        // 2. Lấy các loại Banner Quảng cáo theo Vị trí
        var banners = Antigravity.ECommerce.Services.SCache.GetOrSet("Home_SlideBanners", () => 
            Antigravity.ECommerce.Services.SAdvertising.GetByPosition("Home_Slide"), 60);

        var topBanners = Antigravity.ECommerce.Services.SCache.GetOrSet("Home_TopBanners", () =>
            Antigravity.ECommerce.Services.SAdvertising.GetByPosition("Home_Top"), 60);
            
        var middleBanners = Antigravity.ECommerce.Services.SCache.GetOrSet("Home_MiddleBanners", () =>
            Antigravity.ECommerce.Services.SAdvertising.GetByPosition("Home_Middle"), 60);

        var popupBanners = Antigravity.ECommerce.Services.SCache.GetOrSet("Home_PopupBanners", () =>
            Antigravity.ECommerce.Services.SAdvertising.GetByPosition("Popup"), 60);
            
        // 3. Lấy dữ liệu các Module phụ
        var videos = Antigravity.ECommerce.Services.SCache.GetOrSet("Home_Videos", () => 
            Antigravity.ECommerce.Services.SVideo.GetAll().Where(x => x.Status == 1).Take(3).ToList(), 60);
            
        var news = Antigravity.ECommerce.Services.SCache.GetOrSet("Home_News", () => 
            Antigravity.ECommerce.Services.SNews.GetAll().Where(x => x.Status == 1).Take(3).ToList(), 60);
            
        var faqs = Antigravity.ECommerce.Services.SCache.GetOrSet("Home_FAQs", () => 
            Antigravity.ECommerce.Services.SFAQ.GetAll().Where(x => x.Status == 1).Take(5).ToList(), 60);

        var galleries = Antigravity.ECommerce.Services.SCache.GetOrSet("Home_Galleries", () =>
            Antigravity.ECommerce.Services.SGallery.GetAll().Where(x => x.Status == 1).Take(6).ToList(), 60);

        // 4. Lấy Cấu hình Danh mục hiển thị Trang chủ do Admin thiết lập
        var homeCategories = Antigravity.ECommerce.Services.SCache.GetOrSet("Home_HomeCategories", () =>
            Antigravity.ECommerce.Services.SHomeCategorySetting.GetAll(), 60);

        // Map danh sách sản phẩm tương ứng cho từng Danh mục cấu hình
        var homeCategoryProducts = new Dictionary<int, List<Product>>();
        foreach(var hc in homeCategories)
        {
            // Lấy cả sản phẩm thuộc danh mục con cháu
            var descendantIds = Antigravity.ECommerce.Services.SCategory.GetDescendantIds(hc.CategoryId);
            var categoryIdsStr = string.Join(",", descendantIds);
            
            var products = Antigravity.ECommerce.Services.SCache.GetOrSet($"Home_CategoryProducts_{hc.CategoryId}", () =>
                Antigravity.ECommerce.Framework.FProduct.Search(null, categoryIdsStr, 1, null, null, null, "CreatedAt", "DESC", 1, hc.ProductCount), 60);
            homeCategoryProducts.Add(hc.CategoryId, products ?? new List<Product>());
        }
        
        // Truyền tất cả dữ liệu ra View
        ViewBag.HotProducts = hotProducts;
        ViewBag.Banners = banners;
        ViewBag.TopBanners = topBanners;
        ViewBag.MiddleBanners = middleBanners;
        ViewBag.PopupBanners = popupBanners;
        ViewBag.Videos = videos;
        ViewBag.News = news;
        ViewBag.FAQs = faqs;
        ViewBag.Galleries = galleries;
        ViewBag.HomeCategories = homeCategories;
        ViewBag.HomeCategoryProducts = homeCategoryProducts;
        
        return View();
    }
    #endregion

    #region 2. Các trang hệ thống (System)
    /// <summary>
    /// Trang Chính sách bảo mật (Privacy Policy)
    /// </summary>
    public IActionResult Privacy()
    {
        return View();
    }

    /// <summary>
    /// Trang hiển thị lỗi chung của hệ thống
    /// </summary>
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
    #endregion
}
