using Microsoft.AspNetCore.Mvc;
using Antigravity.ECommerce.Models;
using Antigravity.ECommerce.Framework;
using Antigravity.ECommerce.Services;
using System.Text.Json;
using System.Linq;
using System;
using System.Collections.Generic;

namespace Antigravity.ECommerce.Controllers
{
    public class CartController : Controller
    {
        #region 1. Xử lý Session & Cookie Giỏ hàng
        private const string CART_COOKIE_PREFIX = "UserCart";

        /// <summary>Trả về key giỏ hàng theo tài khoản đang đăng nhập. Khách vãng lai dùng key chung.</summary>
        private string GetCartKey()
        {
            var sessionStr = HttpContext.Request.Cookies["CustomerSession"];
            if (string.IsNullOrEmpty(sessionStr))
                sessionStr = HttpContext.Session.GetString("CustomerSession");
            if (!string.IsNullOrEmpty(sessionStr))
            {
                try
                {
                    var cust = JsonSerializer.Deserialize<CustomerSessionModel>(sessionStr);
                    if (cust != null && cust.CustomerId > 0)
                        return CART_COOKIE_PREFIX + "_" + cust.CustomerId;
                }
                catch { }
            }
            return CART_COOKIE_PREFIX; // Khách vãng lai
        }

        /// <summary>Lấy phí vận chuyển mặc định từ Cài đặt hệ thống</summary>
        private decimal GetDefaultShippingFee()
        {
            var val = SSetting.GetValue("DefaultShippingFee");
            return decimal.TryParse(val, out decimal fee) ? fee : 0;
        }

        /// <summary>Đồng bộ lại giá mới nhất từ DB cho cart</summary>
        private void RefreshCartPrices(List<CartItem> cart)
        {
            foreach (var item in cart)
            {
                var freshProduct = FProduct.GetById(item.ProductId);
                if (freshProduct != null && freshProduct.Status == 1)
                {
                    item.Price = freshProduct.Price;
                    item.ProductName = freshProduct.Name;
                    item.ProductImage = freshProduct.MainImage;
                }
            }
            cart.RemoveAll(x => {
                var p = FProduct.GetById(x.ProductId);
                return p == null || p.Status == 0;
            });
            SaveCartToSession(cart);
        }

        private List<CartItem> GetCartFromSession()
        {
            var key = GetCartKey();
            var cartJson = HttpContext.Request.Cookies[key];
            if (string.IsNullOrEmpty(cartJson)) 
            {
                cartJson = HttpContext.Session.GetString(key);
            }
            // Fallback: nếu user mới login mà chưa có cart riêng, thử đọc cart chung cũ
            if (string.IsNullOrEmpty(cartJson) && key != CART_COOKIE_PREFIX)
            {
                cartJson = HttpContext.Request.Cookies[CART_COOKIE_PREFIX];
                if (string.IsNullOrEmpty(cartJson))
                    cartJson = HttpContext.Session.GetString(CART_COOKIE_PREFIX);
            }
            return cartJson == null ? new List<CartItem>() : JsonSerializer.Deserialize<List<CartItem>>(cartJson) ?? new List<CartItem>();
        }

        private void SaveCartToSession(List<CartItem> cart)
        {
            var key = GetCartKey();
            var json = JsonSerializer.Serialize(cart);
            HttpContext.Session.SetString(key, json);
            
            // Save to cookie for 30 days persistence
            var cookieOptions = new CookieOptions
            {
                Expires = DateTimeOffset.Now.AddDays(30),
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax
            };
            HttpContext.Response.Cookies.Append(key, json, cookieOptions);

            // Nếu đã đăng nhập, xóa giỏ hàng chung (vãng lai) để tránh lẫn dữ liệu
            if (key != CART_COOKIE_PREFIX)
            {
                HttpContext.Session.Remove(CART_COOKIE_PREFIX);
                HttpContext.Response.Cookies.Delete(CART_COOKIE_PREFIX);
            }
        }

        /// <summary>Xóa giỏ hàng hiện tại (dùng sau khi thanh toán xong)</summary>
        private void ClearCart()
        {
            var key = GetCartKey();
            HttpContext.Session.Remove(key);
            HttpContext.Response.Cookies.Delete(key);
            // Xóa luôn giỏ chung nếu có
            HttpContext.Session.Remove(CART_COOKIE_PREFIX);
            HttpContext.Response.Cookies.Delete(CART_COOKIE_PREFIX);
        }
        #endregion

        #region 2. Các thao tác thêm/sửa/xóa Giỏ hàng
        /// <summary>
        /// Trang xem chi tiết Giỏ hàng hiện tại
        /// </summary>
        public IActionResult Index()
        {
            var cart = GetCartFromSession();
            RefreshCartPrices(cart);
            
            var subTotal = cart.Sum(x => x.TotalPrice);
            var shippingFee = subTotal > 0 ? GetDefaultShippingFee() : 0;
            var model = new CartViewModel { 
                Items = cart, 
                SubTotal = subTotal,
                ShippingFee = shippingFee
            };
            return View(model);
        }

        [HttpPost]
        public IActionResult AddToCart(int productId, int quantity = 1)
        {
            var product = FProduct.GetById(productId);
            if (product == null) return Json(new { success = false, message = "Sản phẩm không tồn tại" });

            var cart = GetCartFromSession();
            var item = cart.FirstOrDefault(x => x.ProductId == productId);
            if (item != null)
            {
                item.Quantity += quantity;
            }
            else
            {
                cart.Add(new CartItem {
                    ProductId = productId,
                    ProductName = product.Name,
                    ProductImage = product.MainImage,
                    Price = product.Price,
                    Quantity = quantity
                });
            }

            SaveCartToSession(cart);
            return Json(new { success = true, count = cart.Sum(x => x.Quantity) });
        }

        [HttpPost]
        public IActionResult Remove(int productId)
        {
            var cart = GetCartFromSession();
            cart.RemoveAll(x => x.ProductId == productId);
            SaveCartToSession(cart);
            return Json(new { success = true });
        }

        [HttpPost]
        public IActionResult UpdateQty(int productId, int quantity)
        {
            var cart = GetCartFromSession();
            var item = cart.FirstOrDefault(x => x.ProductId == productId);
            if (item != null)
            {
                item.Quantity = quantity > 0 ? quantity : 1;
                SaveCartToSession(cart);
            }
            return Json(new { success = true });
        }
        #endregion

        #region 3. Thanh toán (Checkout)
        /// <summary>
        /// Trang hiển thị Form điền thông tin Đặt hàng
        /// </summary>
        public IActionResult Checkout()
        {
            var cart = GetCartFromSession();
            if (!cart.Any()) return RedirectToRoute("cart_index");
            RefreshCartPrices(cart);
            
            var subTotal = cart.Sum(x => x.TotalPrice);
            var shippingFee = GetDefaultShippingFee();
            var model = new CartViewModel { 
                Items = cart, 
                SubTotal = subTotal,
                ShippingFee = shippingFee
            };

            var sessionStr = HttpContext.Request.Cookies["CustomerSession"];
            if (string.IsNullOrEmpty(sessionStr)) {
                sessionStr = HttpContext.Session.GetString("CustomerSession");
            }
            if (!string.IsNullOrEmpty(sessionStr)) {
                var customerSession = JsonSerializer.Deserialize<CustomerSessionModel>(sessionStr);
                if (customerSession != null) {
                    var customer = FCustomer.GetById(customerSession.CustomerId);
                    if (customer != null) {
                        model.FullName = customer.FullName;
                        model.Email = customer.Email;
                        model.Phone = customer.Phone;
                        model.Address = customer.Address;
                        model.ProvinceId = customer.ProvinceId;
                        model.WardId = customer.WardId;
                    }
                }
            }

            return View(model);
        }

        [HttpPost]
        public IActionResult ProcessCheckout(CartViewModel postModel)
        {
            var cart = GetCartFromSession();
            if (!cart.Any()) return RedirectToRoute("cart_index");

            // Validate phone number
            if (!System.Text.RegularExpressions.Regex.IsMatch(postModel.Phone ?? "", @"^(0[3|5|7|8|9])+([0-9]{8})$"))
            {
                TempData["ErrorMessage"] = "Số điện thoại không hợp lệ. Vui lòng nhập 10 chữ số bắt đầu bằng 03, 05, 07, 08 hoặc 09.";
                RefreshCartPrices(cart);
                postModel.Items = cart;
                postModel.SubTotal = cart.Sum(x => x.TotalPrice);
                postModel.ShippingFee = GetDefaultShippingFee();
                return View("Checkout", postModel);
            }

            // Validate email if it's provided
            if (!string.IsNullOrEmpty(postModel.Email) && !System.Text.RegularExpressions.Regex.IsMatch(postModel.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                TempData["ErrorMessage"] = "Địa chỉ Email không hợp lệ. Vui lòng kiểm tra lại.";
                RefreshCartPrices(cart);
                postModel.Items = cart;
                postModel.SubTotal = cart.Sum(x => x.TotalPrice);
                postModel.ShippingFee = GetDefaultShippingFee();
                return View("Checkout", postModel);
            }

            Random rnd = new Random();
            string orderCode = "ORD-" + DateTime.Now.ToString("yyMMdd") + rnd.Next(1000, 9999);

            int? customerId = null;
            var sessionStr = HttpContext.Request.Cookies["CustomerSession"];
            if (string.IsNullOrEmpty(sessionStr)) {
                sessionStr = HttpContext.Session.GetString("CustomerSession");
            }
            if (!string.IsNullOrEmpty(sessionStr)) {
                var custSession = JsonSerializer.Deserialize<CustomerSessionModel>(sessionStr);
                if(custSession != null && custSession.CustomerId > 0) customerId = custSession.CustomerId;
            }

            var shippingFee = GetDefaultShippingFee();

            // Create Order
            var order = new Order {
                OrderCode = orderCode,
                CustomerId = customerId,
                CustomerName = postModel.FullName,
                CustomerPhone = postModel.Phone,
                CustomerEmail = postModel.Email,
                ShippingAddress = !string.IsNullOrEmpty(postModel.FullAddress) ? postModel.FullAddress : postModel.Address,
                SubTotal = cart.Sum(x => x.TotalPrice),
                ShippingFee = shippingFee,
                TotalAmount = cart.Sum(x => x.TotalPrice) + shippingFee,
                OrderStatus = 0,
                PaymentMethod = "COD",
                PaymentStatus = 0,
                CustomerNote = postModel.Note
            };

            int orderId = FOrder.Insert(order);
            foreach (var item in cart) {
                FOrder.InsertItem(new OrderItem {
                    OrderId = orderId,
                    ProductId = item.ProductId,
                    ProductName = item.ProductName,
                    Price = item.Price,
                    Quantity = item.Quantity,
                    TotalPrice = item.TotalPrice
                });
            }

            ClearCart();
            
            TempData["OrderCode"] = orderCode;
            return RedirectToRoute("cart_success");
        }

        /// <summary>
        /// Trang thông báo Đặt hàng thành công
        /// </summary>
        public IActionResult Success()
        {
            ViewBag.OrderCode = TempData["OrderCode"];
            return View();
        }
        #endregion
    }
}
