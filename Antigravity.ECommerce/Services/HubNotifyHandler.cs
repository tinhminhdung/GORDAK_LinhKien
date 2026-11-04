using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Antigravity.ECommerce.Models;

namespace Antigravity.ECommerce.Services
{
    /// <summary>
    /// Tiếp nhận yêu cầu "Báo cho tôi" từ khách hàng trên trang chi tiết sản phẩm
    /// Endpoint: POST /api/hub-notify
    /// </summary>
    public static class HubNotifyHandler
    {
        public static async Task<IResult> Handle(HttpContext ctx, IConfiguration cfg, IMemoryCache cache)
        {
            if (!HttpMethods.IsPost(ctx.Request.Method))
            {
                return Results.Json(new { success = false, message = "Phương thức không được hỗ trợ" }, statusCode: 405);
            }

            // Đọc form request
            var form = await ctx.Request.ReadFormAsync().ConfigureAwait(false);
            var req = new HubNotifyRequest
            {
                name = (form["name"].ToString() ?? "").Trim(),
                phone = (form["phone"].ToString() ?? "").Trim(),
                address = (form["address"].ToString() ?? "").Trim(),
                sku = (form["sku"].ToString() ?? "").Trim(),
                note = (form["note"].ToString() ?? "").Trim(),
                hp_address_code = form["hp_address_code"].ToString()
            };

            int qty = 1;
            if (int.TryParse(form["qty"], out int q) && q >= 1 && q <= 999) qty = q;
            req.qty = qty;

            // 1. Chống Bot Spam: Kiểm tra Honeypot
            if (!string.IsNullOrEmpty(req.hp_address_code))
            {
                return Results.Json(new { success = true, message = "Cảm ơn quý khách! Nhật Minh sẽ liên hệ ngay khi có hàng." });
            }

            // 2. Chống Spam: Rate Limit (tối đa 5 lần gửi / IP / giờ)
            string clientIp = ctx.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
            string cacheKey = "NotifyLimit_" + clientIp;
            int count = cache.GetOrCreate(cacheKey, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
                return 0;
            });

            bool isLocal = ctx.Request.Host.Host.Contains("localhost");
            int maxAllowed = isLocal ? 100 : 5;
            if (count >= maxAllowed)
            {
                return Results.Json(new { success = false, message = "Quý khách đã gửi yêu cầu nhiều lần. Vui lòng thử lại sau 1 giờ." });
            }
            cache.Set(cacheKey, count + 1, TimeSpan.FromHours(1));

            // 3. Kiểm tra tính hợp lệ dữ liệu (Server-side validation)
            if (req.name.Length < 2 || req.name.Length > 100)
            {
                return Results.Json(new { success = false, message = "Họ và tên phải từ 2 đến 100 ký tự." });
            }

            string cleanPhone = req.phone.Replace(" ", "").Replace(".", "").Replace("-", "");
            if (cleanPhone.StartsWith("+84")) cleanPhone = "0" + cleanPhone.Substring(3);
            if (cleanPhone.StartsWith("84") && cleanPhone.Length == 11) cleanPhone = "0" + cleanPhone.Substring(2);

            if (!Regex.IsMatch(cleanPhone, @"^0\d{9}$"))
            {
                return Results.Json(new { success = false, message = "Số điện thoại không hợp lệ (yêu cầu 10 chữ số bắt đầu bằng số 0)." });
            }
            req.phone = cleanPhone;

            if (req.address.Length < 5 || req.address.Length > 300)
            {
                return Results.Json(new { success = false, message = "Địa chỉ nhận hàng phải từ 5 đến 300 ký tự." });
            }

            if (string.IsNullOrEmpty(req.sku))
            {
                return Results.Json(new { success = false, message = "Mã sản phẩm không hợp lệ." });
            }

            if ((req.note?.Length ?? 0) > 300)
            {
                req.note = req.note!.Substring(0, 300);
            }

            // 4. Gửi sang HUB (Luồng B)
            var result = await HubSender.SendNotifyAsync(req, cfg).ConfigureAwait(false);

            if (result.Success)
            {
                return Results.Json(new { success = true, message = "Cảm ơn quý khách! Nhật Minh sẽ liên hệ ngay khi có hàng." });
            }
            else
            {
                return Results.Json(new { success = false, message = "Có lỗi xảy ra khi gửi yêu cầu. Quý khách vui lòng thử lại sau!" });
            }
        }
    }
}

