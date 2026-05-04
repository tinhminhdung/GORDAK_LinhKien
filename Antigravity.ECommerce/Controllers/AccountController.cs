using Microsoft.AspNetCore.Mvc;
using Antigravity.ECommerce.Models;
using Antigravity.ECommerce.Services;
using System.Text.Json;

namespace Antigravity.ECommerce.Controllers
{
    public class AccountController : Controller
    {
        [HttpGet]
        public IActionResult Login()
        {
            if (HttpContext.Request.Cookies["CustomerSession"] != null || HttpContext.Session.GetString("CustomerSession") != null)
            {
                return RedirectToAction("Dashboard", "Member");
            }
            return View();
        }

        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Vui lòng nhập tài khoản và mật khẩu.";
                return View();
            }

            var customer = SCustomerAuth.Login(username, password);
            if (customer != null)
            {
                if (customer.Status == 0)
                {
                    ViewBag.Error = "Tài khoản của bạn đã bị khóa.";
                    return View();
                }

                var sessionData = new CustomerSessionModel
                {
                    CustomerId = customer.CustomerId,
                    FullName = customer.FullName,
                    Email = customer.Email,
                    Phone = customer.Phone,
                    Avatar = customer.Avatar,
                    MemberRank = customer.MemberRank,
                    MemberRankName = customer.MemberRankName
                };
                var sessionJson = JsonSerializer.Serialize(sessionData);
                HttpContext.Session.SetString("CustomerSession", sessionJson);
                HttpContext.Response.Cookies.Append("CustomerSession", sessionJson, new CookieOptions { Expires = DateTimeOffset.Now.AddDays(30), HttpOnly = true });
                
                // === GIỎ HÀNG: chuyển giỏ vãng lai sang giỏ riêng của user ===
                MigrateGuestCartToUser(customer.CustomerId);
                
                return RedirectToAction("Dashboard", "Member");
            }

            ViewBag.Error = "Tài khoản hoặc mật khẩu không chính xác.";
            return View();
        }

        [HttpGet]
        public IActionResult Register()
        {
            if (HttpContext.Request.Cookies["CustomerSession"] != null || HttpContext.Session.GetString("CustomerSession") != null)
            {
                return RedirectToAction("Dashboard", "Member");
            }
            return View();
        }

        [HttpPost]
        public IActionResult Register(Customer model, string confirmPassword)
        {
            if (model.Password != confirmPassword)
            {
                ViewBag.Error = "Mật khẩu xác nhận không khớp.";
                return View(model);
            }

            if (string.IsNullOrEmpty(model.Phone) || string.IsNullOrEmpty(model.Password) || string.IsNullOrEmpty(model.FullName))
            {
                ViewBag.Error = "Vui lòng nhập đầy đủ Số điện thoại, Họ tên và Mật khẩu.";
                return View(model);
            }

            try 
            {
                model.Status = 1; // Hoạt động
                int newId = SCustomerAuth.Register(model);
                if (newId > 0)
                {
                    TempData["Success"] = "Đăng ký thành công. Vui lòng đăng nhập.";
                    return RedirectToAction("Login");
                }
                
                ViewBag.Error = "Có lỗi xảy ra, vui lòng thử lại sau.";
                return View(model);
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult Logout()
        {
            // Xóa giỏ hàng chung (vãng lai) trong session để người khác không thấy
            HttpContext.Session.Remove("UserCart");
            // Không xóa cookie UserCart_{id} — để khi user này login lại vẫn còn giỏ hàng riêng

            HttpContext.Session.Remove("CustomerSession");
            HttpContext.Response.Cookies.Delete("CustomerSession");
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            if (HttpContext.Request.Cookies["CustomerSession"] != null || HttpContext.Session.GetString("CustomerSession") != null)
            {
                return RedirectToAction("Dashboard", "Member");
            }
            return View();
        }

        [HttpPost]
        public IActionResult ForgotPassword(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                ViewBag.Error = "Vui lòng nhập email.";
                return View();
            }

            var customer = SCustomerAuth.GetByEmailOrPhone(email);
            if (customer == null || string.IsNullOrEmpty(customer.Email))
            {
                ViewBag.Error = "Không tìm thấy tài khoản với email này.";
                return View();
            }

            // Generate new password
            string newPassword = Guid.NewGuid().ToString("N").Substring(0, 8) + "@A1";
            string hash = SCustomerAuth.HashPassword(newPassword);
            
            // Update password in DB
            Antigravity.ECommerce.Framework.FCustomer.UpdatePassword(customer.CustomerId, hash);

            // Send email using template
            var replacements = new Dictionary<string, string>
            {
                { "CustomerName", customer.FullName },
                { "NewPassword", newPassword }
            };
            var (sent, error) = SEmailSender.SendFromTemplate("PasswordReset", customer.Email, "Yêu cầu khôi phục mật khẩu", replacements);
            if (sent)
            {
                TempData["Success"] = "Mật khẩu mới đã được gửi vào email của bạn.";
                return RedirectToAction("Login");
            }
            else
            {
                // Password was already changed, inform user
                TempData["Success"] = "Mật khẩu mới đã được tạo nhưng không gửi được email. Vui lòng liên hệ quản trị viên.";
                return RedirectToAction("Login");
            }
        }

        /// <summary>
        /// Chuyển giỏ hàng vãng lai (UserCart) sang giỏ riêng của user (đã đăng nhập).
        /// Nếu user đã có giỏ riêng từ lần trước, merge cả hai lại.
        /// </summary>
        private void MigrateGuestCartToUser(int customerId)
        {
            try
            {
                string userKey = "UserCart_" + customerId;
                string guestKey = "UserCart";

                // Đọc giỏ vãng lai
                var guestJson = HttpContext.Request.Cookies[guestKey];
                if (string.IsNullOrEmpty(guestJson))
                    guestJson = HttpContext.Session.GetString(guestKey);
                var guestCart = string.IsNullOrEmpty(guestJson)
                    ? new List<CartItem>()
                    : JsonSerializer.Deserialize<List<CartItem>>(guestJson) ?? new List<CartItem>();

                // Đọc giỏ riêng của user (từ cookie cũ lần login trước)
                var userJson = HttpContext.Request.Cookies[userKey];
                var userCart = string.IsNullOrEmpty(userJson)
                    ? new List<CartItem>()
                    : JsonSerializer.Deserialize<List<CartItem>>(userJson) ?? new List<CartItem>();

                // Merge: thêm sản phẩm vãng lai vào giỏ user (cộng dồn nếu trùng)
                foreach (var guestItem in guestCart)
                {
                    var existing = userCart.FirstOrDefault(x => x.ProductId == guestItem.ProductId);
                    if (existing != null)
                        existing.Quantity += guestItem.Quantity;
                    else
                        userCart.Add(guestItem);
                }

                // Lưu giỏ riêng của user
                var mergedJson = JsonSerializer.Serialize(userCart);
                HttpContext.Session.SetString(userKey, mergedJson);
                HttpContext.Response.Cookies.Append(userKey, mergedJson, new CookieOptions
                {
                    Expires = DateTimeOffset.Now.AddDays(30),
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Lax
                });

                // Xóa giỏ vãng lai
                HttpContext.Session.Remove(guestKey);
                HttpContext.Response.Cookies.Delete(guestKey);
            }
            catch { /* Không để lỗi giỏ hàng chặn luồng login */ }
        }
    }
}
