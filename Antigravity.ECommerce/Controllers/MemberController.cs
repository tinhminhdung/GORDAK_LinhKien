using Microsoft.AspNetCore.Mvc;
using Antigravity.ECommerce.Models;
using Antigravity.ECommerce.Services;
using Antigravity.ECommerce.Framework;
using System.Text.Json;
using Microsoft.Data.SqlClient;

namespace Antigravity.ECommerce.Controllers
{
    public class MemberController : Controller
    {
        #region 1. Helper & Filters
        private CustomerSessionModel? GetCurrentSession()
        {
            var sessionStr = HttpContext.Request.Cookies["CustomerSession"];
            if (string.IsNullOrEmpty(sessionStr)) {
                sessionStr = HttpContext.Session.GetString("CustomerSession");
            }
            if (string.IsNullOrEmpty(sessionStr)) return null;
            try {
                return JsonSerializer.Deserialize<CustomerSessionModel>(sessionStr);
            } catch {
                return null;
            }
        }

        private Customer? GetCurrentCustomer()
        {
            var session = GetCurrentSession();
            if (session == null) return null;
            return SCustomer.GetById(session.CustomerId);
        }

        // Action Filter thay thế để kiểm tra đăng nhập
        public override void OnActionExecuting(Microsoft.AspNetCore.Mvc.Filters.ActionExecutingContext context)
        {
            var session = GetCurrentSession();
            if (session == null)
            {
                context.Result = new RedirectToActionResult("Login", "Account", null);
            }
            base.OnActionExecuting(context);
        }
        #endregion

        #region 2. Dashboard & Thông tin chung
        /// <summary>
        /// Bảng điều khiển chính của Thành viên sau khi đăng nhập
        /// </summary>
        [HttpGet]
        public IActionResult Dashboard()
        {
            var customer = GetCurrentCustomer();
            if (customer == null) return RedirectToAction("Login", "Account");

            var orders = SCustomer.GetOrderHistory(customer.CustomerId, page: 1, size: 5);
            
            // Query real stats from Orders table
            var stats = BaseConnectionSql.QuerySingle<OrderStatsModel>(
                "SELECT COUNT(*) AS TotalOrders, ISNULL(SUM(TotalAmount),0) AS TotalSpent FROM Orders WHERE CustomerId = @Id",
                new Microsoft.Data.SqlClient.SqlParameter("@Id", customer.CustomerId));
            
            ViewBag.Customer = customer;
            ViewBag.RecentOrders = orders;
            ViewBag.RealTotalOrders = stats?.TotalOrders ?? 0;
            ViewBag.RealTotalSpent = stats?.TotalSpent ?? 0m;

            return View();
        }

        [HttpGet]
        public IActionResult MyReviews(int page = 1)
        {
            var customer = GetCurrentCustomer();
            if (customer == null) return RedirectToAction("Login", "Account");

            ViewBag.Customer = customer;
            var reviews = FReview.GetByCustomerId(customer.CustomerId, page);
            ViewBag.PageIndex = page;
            ViewBag.TotalCount = reviews.Count > 0 ? reviews[0].TotalCount : 0;
            ViewBag.TotalPages = (int)Math.Ceiling((double)ViewBag.TotalCount / 20);
            return View(reviews);
        }
        #endregion

        #region 3. Quản lý Hồ sơ (Profile) & Mật khẩu
        /// <summary>
        /// Trang xem và sửa Thông tin cá nhân
        /// </summary>
        [HttpGet]
        public IActionResult Profile()
        {
            var customer = GetCurrentCustomer();
            if (customer == null) return RedirectToAction("Login", "Account");

            return View(customer);
        }

        [HttpPost]
        public async Task<IActionResult> Profile(Customer model, IFormFile? AvatarFile)
        {
            var session = GetCurrentSession();
            if (session == null) return RedirectToAction("Login", "Account");

            model.CustomerId = session.CustomerId; // Bảo vệ, tránh đổi ID người khác

            if (AvatarFile != null && AvatarFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "avatars");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }
                var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(AvatarFile.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await AvatarFile.CopyToAsync(fileStream);
                }
                model.Avatar = "/uploads/avatars/" + uniqueFileName;
            }
            else
            {
                // Giữ lại avatar cũ
                var existingCustomer = SCustomer.GetById(session.CustomerId);
                if (existingCustomer != null)
                {
                    model.Avatar = existingCustomer.Avatar;
                }
            }

            int result = FCustomer.UpdateProfile(model);
            if (result != 0)
            {
                // Cập nhật session
                var customer = SCustomer.GetById(session.CustomerId);
                if (customer != null)
                {
                    var newSession = new CustomerSessionModel
                    {
                        CustomerId = customer.CustomerId,
                        FullName = customer.FullName,
                        Email = customer.Email,
                        Phone = customer.Phone,
                        Avatar = customer.Avatar,
                        MemberRank = customer.MemberRank,
                        MemberRankName = customer.MemberRankName
                    };
                    var sessionJson = JsonSerializer.Serialize(newSession);
                    HttpContext.Session.SetString("CustomerSession", sessionJson);
                    HttpContext.Response.Cookies.Append("CustomerSession", sessionJson, new CookieOptions { Expires = DateTimeOffset.Now.AddDays(30), HttpOnly = true });
                }
                TempData["Success"] = "Cập nhật thông tin thành công.";
            }
            else
            {
                TempData["Error"] = "Cập nhật thất bại. Vui lòng thử lại.";
            }

            return RedirectToAction("Profile");
        }
        #endregion

        #region 4. Quản lý Đơn hàng
        /// <summary>
        /// Lịch sử mua hàng của Thành viên
        /// </summary>
        [HttpGet]
        public IActionResult Orders()
        {
            var session = GetCurrentSession();
            if (session == null) return RedirectToAction("Login", "Account");

            var orders = SCustomer.GetOrderHistory(session.CustomerId, page: 1, size: 50); // Mặc định hiển thị 50 đơn
            return View(orders);
        }

        [HttpGet]
        public IActionResult OrderDetail(int id)
        {
            var session = GetCurrentSession();
            if (session == null) return RedirectToAction("Login", "Account");

            var order = FOrder.GetById(id);
            if (order == null || order.CustomerId != session.CustomerId)
            {
                return RedirectToAction("Orders");
            }

            var orderItems = FOrder.GetItemsByOrderId(id);
            ViewBag.OrderItems = orderItems;

            return View(order);
        }

        [HttpPost]
        public IActionResult Reorder(int orderId)
        {
            var session = GetCurrentSession();
            if (session == null) return RedirectToAction("Login", "Account");

            var order = FOrder.GetById(orderId);
            if (order == null || order.CustomerId != session.CustomerId)
            {
                return RedirectToAction("Orders");
            }

            var items = FOrder.GetItemsByOrderId(orderId);
            if (items != null)
            {
                string cartKey = "UserCart_" + session.CustomerId;
                var cartJson = HttpContext.Request.Cookies[cartKey];
                if (string.IsNullOrEmpty(cartJson))
                    cartJson = HttpContext.Session.GetString(cartKey);
                var cart = string.IsNullOrEmpty(cartJson) ? new System.Collections.Generic.List<CartItem>() : JsonSerializer.Deserialize<System.Collections.Generic.List<CartItem>>(cartJson) ?? new System.Collections.Generic.List<CartItem>();

                foreach (var item in items)
                {
                    var product = FProduct.GetById(item.ProductId);
                    if (product != null)
                    {
                        var cartItem = cart.FirstOrDefault(x => x.ProductId == item.ProductId);
                        if (cartItem != null)
                        {
                            cartItem.Quantity += item.Quantity;
                        }
                        else
                        {
                            cart.Add(new CartItem {
                                ProductId = product.ProductId,
                                ProductName = product.Name,
                                ProductImage = product.MainImage,
                                Price = product.Price,
                                Quantity = item.Quantity
                            });
                        }
                    }
                }
                
                var json = JsonSerializer.Serialize(cart);
                HttpContext.Session.SetString(cartKey, json);
                HttpContext.Response.Cookies.Append(cartKey, json, new CookieOptions
                {
                    Expires = DateTimeOffset.Now.AddDays(30),
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Lax
                });
            }

            return RedirectToAction("Index", "Cart");
        }

        /// <summary>
        /// Trang đổi mật khẩu
        /// </summary>
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ChangePassword(string currentPassword, string newPassword, string confirmPassword)
        {
            if (newPassword != confirmPassword)
            {
                TempData["Error"] = "Mật khẩu xác nhận không khớp.";
                return View();
            }

            var session = GetCurrentSession();
            if (session == null) return RedirectToAction("Login", "Account");

            var customer = SCustomer.GetById(session.CustomerId);
            if (customer != null && SCustomerAuth.VerifyPassword(currentPassword, customer.Password ?? ""))
            {
                string hashed = SCustomerAuth.HashPassword(newPassword);
                FCustomer.UpdatePassword(customer.CustomerId, hashed);
                TempData["Success"] = "Đổi mật khẩu thành công.";
                return RedirectToAction("Dashboard");
            }

            TempData["Error"] = "Mật khẩu hiện tại không đúng.";
            return View();
        }
        #endregion

        #region 5. Yêu thích (Wishlist)
        /// <summary>
        /// Trang danh sách sản phẩm yêu thích đã lưu
        /// </summary>
        [HttpGet]
        public IActionResult Wishlist()
        {
            var session = GetCurrentSession();
            if (session == null) return RedirectToAction("Login", "Account");

            var items = SWishlist.GetByCustomerId(session.CustomerId);
            return View(items);
        }

        [HttpGet]
        public IActionResult GetWishlistIds()
        {
            var session = GetCurrentSession();
            if (session == null) return Json(new int[0]);

            var items = SWishlist.GetByCustomerId(session.CustomerId);
            return Json(items.Select(x => x.ProductId).ToList());
        }

        [HttpPost]
        public IActionResult ToggleWishlist(int productId)
        {
            var session = GetCurrentSession();
            if (session == null) return Json(new { success = false, message = "Vui lòng đăng nhập" });

            int result = SWishlist.Toggle(session.CustomerId, productId);
            return Json(new { success = true, action = result == 1 ? "added" : "removed" });
        }
        #endregion
    }
}
