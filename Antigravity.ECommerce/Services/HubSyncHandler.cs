using System;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Antigravity.ECommerce.Models;

namespace Antigravity.ECommerce.Services
{
    /// <summary>
    /// Tiếp nhận dữ liệu tồn kho, giá bán, ĐVT và ngày dự kiến hàng về từ máy chủ HUB (Luồng A)
    /// Endpoint: POST /hub-sync
    /// </summary>
    public static class HubSyncHandler
    {
        public static async Task<IResult> Handle(HttpContext ctx, IConfiguration cfg)
        {
            // 1. Chỉ chấp nhận phương thức POST
            if (!HttpMethods.IsPost(ctx.Request.Method))
            {
                return Results.NotFound();
            }

            // 2. Bắt buộc kết nối an toàn HTTPS (cho phép localhost khi lập trình)
            if (!ctx.Request.IsHttps && !ctx.Request.Host.Host.Contains("localhost"))
            {
                return Results.NotFound();
            }

            // 3. Kiểm tra IP nguồn máy chủ HUB
            string? allowIp = SSetting.GetValue("Hub_SyncAllowIp");
            if (string.IsNullOrEmpty(allowIp)) allowIp = cfg["HubSettings:SyncAllowIp"];
            if (!string.IsNullOrEmpty(allowIp))
            {
                string? clientIp = ctx.Connection.RemoteIpAddress?.MapToIPv4().ToString();
                bool isAllowed = false;
                var allowedList = allowIp.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var ip in allowedList)
                {
                    string trimmed = ip.Trim();
                    if (clientIp == trimmed) { isAllowed = true; break; }
                    if (trimmed == "127.0.0.1" && (clientIp == "127.0.0.1" || clientIp == "::1")) { isAllowed = true; break; }
                }

                if (!isAllowed)
                {
                    return Results.NotFound();
                }
            }

            // 4. Giới hạn dung lượng yêu cầu <= 2 MB
            if (ctx.Request.ContentLength > 2_000_000)
            {
                return Results.BadRequest("error");
            }

            // 5. Đọc body thô dạng byte[]
            using var ms = new MemoryStream();
            await ctx.Request.Body.CopyToAsync(ms).ConfigureAwait(false);
            byte[] body = ms.ToArray();
            if (body.Length == 0)
            {
                return Results.BadRequest("error");
            }

            // 6. Kiểm tra Timestamp và chữ ký số HMAC-SHA256
            string ts = ctx.Request.Headers["X-Hub-Timestamp"].ToString();
            string sig = ctx.Request.Headers["X-Hub-Signature"].ToString();
            string? key = SSetting.GetValue("Hub_SyncKey");
            if (string.IsNullOrEmpty(key)) key = cfg["HubSettings:SyncKey"];

            if (string.IsNullOrEmpty(key))
            {
                return Results.Unauthorized();
            }

            if (!long.TryParse(ts, out var t) || Math.Abs(DateTimeOffset.UtcNow.ToUnixTimeSeconds() - t) > 300)
            {
                return Results.Unauthorized();
            }

            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
            byte[] pre = Encoding.UTF8.GetBytes(ts + ".");
            byte[] all = new byte[pre.Length + body.Length];
            Buffer.BlockCopy(pre, 0, all, 0, pre.Length);
            Buffer.BlockCopy(body, 0, all, pre.Length, body.Length);
            string expectSig = Convert.ToHexString(hmac.ComputeHash(all)).ToLowerInvariant();

            if (!CryptographicOperations.FixedTimeEquals(Encoding.ASCII.GetBytes(expectSig), Encoding.ASCII.GetBytes(sig.ToLowerInvariant())))
            {
                return Results.Unauthorized();
            }

            // 7. Giải mã dữ liệu JSON
            HubSyncPayload? payload;
            try
            {
                payload = JsonSerializer.Deserialize<HubSyncPayload>(body);
            }
            catch
            {
                return Results.BadRequest("error");
            }

            if (payload == null || payload.items == null || payload.items.Count > 1000)
            {
                return Results.BadRequest("error");
            }

            // 8. Cập nhật dữ liệu vào CSDL qua Stored Procedure
            int updated = 0, unknown = 0;
            try
            {
                foreach (var it in payload.items)
                {
                    if (string.IsNullOrEmpty(it.code) || it.code.Length > 50 || it.stock < 0 || it.price < 0 || (it.unit?.Length ?? 0) > 30)
                    {
                        continue;
                    }

                    DateTime? expectedDate = null;
                    if (!string.IsNullOrEmpty(it.expected))
                    {
                        if (DateTime.TryParseExact(it.expected.Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d))
                        {
                            expectedDate = d;
                        }
                        else
                        {
                            continue; // Bỏ dòng nếu sai định dạng ngày
                        }
                    }

                    // Gọi Stored Procedure SP_Hub_SyncProduct (Compact Single-line)
                    var prms = new SqlParameter[] { new SqlParameter("@SKU", it.code.Trim()), new SqlParameter("@Stock", it.stock), new SqlParameter("@Price", it.price), new SqlParameter("@Unit", (object?)it.unit ?? DBNull.Value), new SqlParameter("@ExpectedDate", (object?)expectedDate ?? DBNull.Value) };
                    var res = BaseConnectionSql.ExecuteScalar("SP_Hub_SyncProduct", prms);
                    if (res != null && Convert.ToInt32(res) > 0)
                    {
                        updated++;
                    }
                    else
                    {
                        unknown++;
                    }
                }

                if (updated > 0)
                {
                    SCache.Remove("Products_All");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("HubSync Error: " + ex.Message);
                return Results.Problem(detail: "error", statusCode: 500);
            }

            // 9. Trả về kết quả JSON theo chuẩn HUB
            return Results.Json(new { ok = true, updated, unknown });
        }
    }
}
