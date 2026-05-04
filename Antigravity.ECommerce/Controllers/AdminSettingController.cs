using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Antigravity.ECommerce.Models;
using Antigravity.ECommerce.Services;
using Antigravity.ECommerce.Framework;

namespace Antigravity.ECommerce.Controllers
{
    [Authorize]
    [Permission("Settings", ActionType.View)]
    public class AdminSettingController : Controller
    {
        public IActionResult Index()
        {
            var model = SSetting.GetViewModel();
            return View(model);
        }

        [HttpPost]
        [Permission("Settings", ActionType.Edit)]
        public IActionResult Save(GlobalSettingsViewModel model)
        {
            try
            {
                SSetting.SaveSettings(model, User.Identity?.Name ?? "Admin");
                SSeo.RefreshSitemapAndCache();
                
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = true, message = "Lưu thành công!" });
                }

                TempData["SuccessMessage"] = "Cấu hình hệ thống đã được cập nhật thành công.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "Lỗi khi lưu cấu hình: " + ex.Message });
                }
                TempData["ErrorMessage"] = "Lỗi khi lưu cấu hình: " + ex.Message;
                return View("Index", model);
            }
        }

        /// <summary> Xóa toàn bộ Cache thủ công từ Admin </summary>
        [HttpPost]
        [Permission("Settings", ActionType.Edit)]
        public IActionResult ClearCache()
        {
            try
            {
                SCache.ClearAll();
                return Json(new { success = true, message = "Đã xóa toàn bộ cache hệ thống!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary> Tạo sitemap.xml vật lý cho Google (Full SEO với Ảnh) </summary>
        [HttpPost]
        [Permission("Settings", ActionType.Edit)]
        public IActionResult GenerateSitemap()
        {
            try
            {
                var baseUrl = $"{this.Request.Scheme}://{this.Request.Host}";
                SSeo.GenerateFullSitemap(baseUrl);
                return Json(new { success = true, message = "Đã tạo xong sitemap.xml vật lý!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [Permission("Settings", ActionType.View)]
        public async Task<IActionResult> CheckGeminiApi([FromBody] string apiKey)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
                return Json(new { success = false, message = "API Key không được để trống." });

            try
            {
                apiKey = apiKey.Trim();
                using var httpClient = new HttpClient();
                httpClient.Timeout = TimeSpan.FromSeconds(15);
                
                // Sử dụng GET request lấy danh sách model để test Key, an toàn và chính xác hơn POST generateContent
                var geminiUrl = $"https://generativelanguage.googleapis.com/v1beta/models?key={apiKey}";
                var response = await httpClient.GetAsync(geminiUrl);

                if (response.IsSuccessStatusCode)
                {
                    return Json(new { success = true, message = "Kết nối thành công (API Key hợp lệ)." });
                }
                else
                {
                    var errorMsg = await response.Content.ReadAsStringAsync();
                    return Json(new { success = false, message = "API Key không hợp lệ hoặc bị Google từ chối." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi kết nối: " + ex.Message });
            }
        }
    }
}
