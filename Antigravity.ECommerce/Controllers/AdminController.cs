using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Antigravity.ECommerce.Services;
using Antigravity.ECommerce.Models;
using Antigravity.ECommerce.Framework;

namespace Antigravity.ECommerce.Controllers
{
    public class AdminController : Controller
    {
        [AllowAnonymous]
        public IActionResult AccessDenied(string module)
        {
            ViewBag.Module = module;
            return View();
        }

        [AllowAnonymous]
        public IActionResult TestDb()
        {
            var data = Antigravity.ECommerce.Services.BaseConnectionSql.Query<dynamic>("SELECT Username, Password, LEN(Password) as PassLen FROM AdminUsers", null);
            return Json(data);
        }

        private static System.Security.Claims.Claim CreateClaim(string type, string value)
        {
            return new System.Security.Claims.Claim(type, value);
        }

        // ── Dashboard (requires login) ──
        [Permission("Dashboard", ActionType.View)]
        public IActionResult Index()
        {
            ViewBag.Summary = SDashboard.GetSummary();
            ViewBag.Views7d = SDashboard.GetViewsByDay(7);
            ViewBag.TrafficSource = SDashboard.GetTrafficSource();
            ViewBag.RevenueData = SDashboard.GetRevenueByDay(365);
            ViewBag.TopProducts = SDashboard.GetTopProducts(10);
            ViewBag.TopCategories = SDashboard.GetTopCategories(5);
            ViewBag.RecentOrders = SDashboard.GetRecentOrders(5);
            ViewBag.LowStock = SDashboard.GetLowStock(10);
            ViewBag.RecentContacts = FContact.Search(null, null, null, "CreatedAt", "DESC", 1, 5);
            ViewBag.RecentNews = SNews.Search(null, null, null, "CreatedAt", "DESC", 1, 5);
            return View();
        }

        [Authorize]
        public IActionResult Welcome()
        {
            return View();
        }

        // ── Login GET ──
        [AllowAnonymous]
        public async Task<IActionResult> Login(string? error = null, string? returnUrl = null)
        {
            if (!string.IsNullOrEmpty(error))
            {
                // Nếu có lỗi (thường là bị khóa), bắt buộc SignOut để xóa Cookie cũ
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                ViewBag.Error = error;
            }
            else if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index");
            }
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // ── Login POST ──
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username, string password, string? returnUrl = null)
        {
            var user = FAdminUser.Login(username, password);

            if (user != null)
            {
                var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
                identity.AddClaim(CreateClaim(ClaimTypes.Name, user.Username));
                identity.AddClaim(CreateClaim(ClaimTypes.NameIdentifier, user.UserId.ToString()));
                identity.AddClaim(CreateClaim(ClaimTypes.Role, user.RoleName ?? "Admin"));
                identity.AddClaim(CreateClaim("FullName", user.FullName ?? ""));
                identity.AddClaim(CreateClaim("RoleId", user.RoleId?.ToString() ?? "0"));
                identity.AddClaim(CreateClaim("Avatar", user.Avatar ?? ""));

                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal,
                    new AuthenticationProperties { IsPersistent = true });

                try {
                    FAdminLog.Insert(new AdminLog {
                        Username = user.Username, Action = "Login", Module = "Auth",
                        Description = "Đăng nhập thành công vào hệ thống",
                        IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? ""
                    });
                } catch { }


                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);

                return RedirectToAction("Welcome");
            }

            ViewBag.Error = "Tên đăng nhập hoặc mật khẩu không đúng!";
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // ── Logout ──
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        // ── Save Theme via Ajax ──
        [HttpPost]
        [Authorize]
        public IActionResult SaveTheme(string layout, string scheme, string sidebar, string sidebarSize, string topbar)
        {
            try
            {
                var username = User.Identity?.Name ?? "system";
                if (!string.IsNullOrEmpty(layout)) Framework.FSetting.UpdateValue("ThemeLayout", layout, username);
                if (!string.IsNullOrEmpty(scheme)) Framework.FSetting.UpdateValue("ThemeColorScheme", scheme, username);
                if (!string.IsNullOrEmpty(sidebar)) Framework.FSetting.UpdateValue("ThemeSidebar", sidebar, username);
                if (!string.IsNullOrEmpty(sidebarSize)) Framework.FSetting.UpdateValue("ThemeSidebarSize", sidebarSize, username);
                if (!string.IsNullOrEmpty(topbar)) Framework.FSetting.UpdateValue("ThemeTopbar", topbar, username);
                
                return Json(new { success = true, message = "Lưu cấu hình giao diện thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // ── Get Email Errors via Ajax ──
        [HttpGet]
        [Authorize]
        public IActionResult GetEmailErrors()
        {
            var errors = new List<string>();
            while (SEmailSender.ErrorQueue.TryDequeue(out var error))
            {
                errors.Add(error);
            }
            return Json(errors);
        }
    }
}
