using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Antigravity.ECommerce.Framework;
using Antigravity.ECommerce.Models;
using Antigravity.ECommerce.Services;

namespace Antigravity.ECommerce.Controllers
{
    [Authorize]
    [Permission("Orders", ActionType.View)]
    public class AdminHubNotifyController : Controller
    {
        public IActionResult Index(
            string kw = "",
            int? status = null,
            int? hubStatus = null,
            DateTime? dateMin = null,
            DateTime? dateMax = null,
            int page = 1,
            int size = 20)
        {
            var data = FHubNotify.Search(kw, status, hubStatus, dateMin, dateMax, page, size);

            ViewBag.Keyword = kw;
            ViewBag.Status = status;
            ViewBag.HubStatus = hubStatus;
            ViewBag.DateMin = dateMin?.ToString("yyyy-MM-dd");
            ViewBag.DateMax = dateMax?.ToString("yyyy-MM-dd");
            ViewBag.PageIndex = page;
            ViewBag.PageSize = size;
            ViewBag.TotalCount = data.Count > 0 ? data[0].TotalCount : 0;
            ViewBag.TotalPages = (int)Math.Ceiling((double)ViewBag.TotalCount / size);

            var counts = FHubNotify.GetStatusCounts();
            ViewBag.CountAll = counts.All;
            ViewBag.CountNew = counts.New;
            ViewBag.CountContacted = counts.Contacted;
            ViewBag.CountCompleted = counts.Completed;
            ViewBag.CountCancelled = counts.Cancelled;

            return View(data);
        }

        [HttpPost]
        [Permission("Orders", ActionType.Edit)]
        public IActionResult UpdateStatus(int id, int status, string? note = null)
        {
            try
            {
                FHubNotify.UpdateStatus(id, status, note);
                return Json(new { success = true, message = "Đã cập nhật trạng thái thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [Permission("Orders", ActionType.Delete)]
        public IActionResult Delete(int id)
        {
            try
            {
                FHubNotify.DeleteById(id);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [Permission("Orders", ActionType.Delete)]
        public IActionResult BulkDelete([FromBody] List<int> ids)
        {
            if (ids == null || ids.Count == 0)
            {
                return Json(new { success = false, message = "Chưa chọn bản ghi nào để xóa." });
            }

            try
            {
                foreach (var id in ids)
                {
                    FHubNotify.DeleteById(id);
                }
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SyncToHub(int id)
        {
            var config = HttpContext.RequestServices.GetRequiredService<IConfiguration>();
            var res = await HubSender.ResendNotifyByIdAsync(id, config);
            return Json(new { success = res.Success, message = res.Message, statusCode = res.StatusCode });
        }

        [HttpPost]
        public async Task<IActionResult> TestHubConnection()
        {
            var cfg = HttpContext.RequestServices.GetRequiredService<IConfiguration>();
            string source = SSetting.GetValue("Hub_Source");
            if (string.IsNullOrEmpty(source)) source = cfg["HubSettings:Source"] ?? "gordak";

            string url = SSetting.GetValue("Hub_NotifyUrl");
            if (string.IsNullOrEmpty(url)) url = SSetting.GetValue("Hub_OrderUrl");
            if (string.IsNullOrEmpty(url)) url = cfg["HubSettings:NotifyUrl"] ?? cfg["HubSettings:OrderUrl"] ?? "https://pos.nhatminhesc.com/api/web/orders";

            string orderKey = SSetting.GetValue("Hub_NotifyKey");
            if (string.IsNullOrEmpty(orderKey)) orderKey = SSetting.GetValue("Hub_OrderKey");
            if (string.IsNullOrEmpty(orderKey)) orderKey = cfg["HubSettings:NotifyKey"] ?? cfg["HubSettings:OrderKey"] ?? "";

            string syncKey = SSetting.GetValue("Hub_SyncKey");
            if (string.IsNullOrEmpty(syncKey)) syncKey = cfg["HubSettings:SyncKey"] ?? "";

            string syncAllowIp = SSetting.GetValue("Hub_SyncAllowIp");
            if (string.IsNullOrEmpty(syncAllowIp)) syncAllowIp = cfg["HubSettings:SyncAllowIp"] ?? "";

            string publicIp = "Không xác định";
            try
            {
                using var ipClient = new System.Net.Http.HttpClient { Timeout = TimeSpan.FromSeconds(5) };
                publicIp = (await ipClient.GetStringAsync("https://api.ipify.org")).Trim();
            }
            catch { }

            var testPayload = new Dictionary<string, object>
            {
                { "kind", "notify" },
                { "source", source },
                { "orderCode", "NTFTEST" + DateTime.Now.ToString("yyMMddHHmmss") },
                { "createdAt", DateTimeOffset.Now.ToString("yyyy-MM-ddTHH:mm:sszzz") },
                {
                    "customer", new Dictionary<string, object>
                    {
                        { "name", "Khach Test Ket Noi HUB" },
                        { "phone", "0912345678" },
                        { "address", "Ha Noi" }
                    }
                },
                {
                    "items", new List<object>
                    {
                        new
                        {
                            sku = "TEST-SKU",
                            name = "San pham kiem tra ket noi HUB",
                            qty = 1,
                            price = 100000,
                            unit = "Chiếc",
                            expected = (string?)null
                        }
                    }
                },
                { "note", "Test ket noi tu trang quan tri Admin" }
            };

            string json = System.Text.Json.JsonSerializer.Serialize(testPayload);
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var sendRes = await HubSender.SendJsonAsync(json, url, orderKey);
            sw.Stop();

            bool sameKeyWarning = !string.IsNullOrEmpty(orderKey) && orderKey == syncKey;
            string maskedOrderKey = string.IsNullOrEmpty(orderKey)
                ? "(Chưa cấu hình)"
                : (orderKey.Length > 12 ? orderKey.Substring(0, 8) + "..." + orderKey.Substring(orderKey.Length - 6) + $" ({orderKey.Length} ký tự)" : orderKey);

            return Json(new
            {
                success = sendRes.Success,
                statusCode = sendRes.StatusCode,
                hubResponse = sendRes.Message,
                latencyMs = sw.ElapsedMilliseconds,
                source = source,
                url = url,
                maskedOrderKey = maskedOrderKey,
                syncAllowIp = string.IsNullOrEmpty(syncAllowIp) ? "(Cho phép mọi IP)" : syncAllowIp,
                publicIp = publicIp,
                sameKeyWarning = sameKeyWarning,
                serverTime = DateTimeOffset.Now.ToString("dd/MM/yyyy HH:mm:ss zzz")
            });
        }

        [HttpGet]
        public IActionResult ExportCSV(
            string kw = "",
            int? status = null,
            int? hubStatus = null,
            DateTime? dateMin = null,
            DateTime? dateMax = null)
        {
            var list = FHubNotify.Search(kw, status, hubStatus, dateMin, dateMax, 1, 10000);
            var sb = new StringBuilder();
            sb.AppendLine("ID,Mã phiếu,Ngày đăng ký,Họ tên KH,Số điện thoại,Địa chỉ,Mã SKU,Tên sản phẩm,Số lượng,Đơn vị,Đơn giá (Chưa VAT),Ngày dự kiến,Ghi chú,Trạng thái xử lý,Đã gửi HUB,Số lần thử HUB,Lỗi HUB");

            foreach (var item in list)
            {
                string statusText = item.Status switch
                {
                    0 => "Mới chờ xử lý",
                    1 => "Đã liên hệ",
                    2 => "Đã báo có hàng",
                    3 => "Đã hủy",
                    _ => "Mới"
                };

                sb.Append($"{item.Id},");
                sb.Append($"{EscapeCsv(item.OrderCode)},");
                sb.Append($"{item.CreatedAt:yyyy-MM-dd HH:mm:ss},");
                sb.Append($"{EscapeCsv(item.CustomerName)},");
                sb.Append($"{EscapeCsv(item.CustomerPhone)},");
                sb.Append($"{EscapeCsv(item.CustomerAddress)},");
                sb.Append($"{EscapeCsv(item.ProductSKU)},");
                sb.Append($"{EscapeCsv(item.ProductName)},");
                sb.Append($"{item.Quantity},");
                sb.Append($"{EscapeCsv(item.Unit ?? "Chiếc")},");
                sb.Append($"{item.Price:0},");
                sb.Append($"{(item.ExpectedDate.HasValue ? item.ExpectedDate.Value.ToString("yyyy-MM-dd") : "")},");
                sb.Append($"{EscapeCsv(item.Note)},");
                sb.Append($"{EscapeCsv(statusText)},");
                sb.Append($"{(item.HubSent ? "Đã gửi" : "Chưa gửi")},");
                sb.Append($"{item.HubTries},");
                sb.AppendLine($"{EscapeCsv(item.HubLastError)}");
            }

            byte[] bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
            return File(bytes, "text/csv", $"DangKyBaoCoHang_{DateTime.Now:yyyyMMddHHmm}.csv");
        }

        private static string EscapeCsv(string? val)
        {
            if (string.IsNullOrEmpty(val)) return "\"\"";
            return "\"" + val.Replace("\"", "\"\"").Replace("\r", " ").Replace("\n", " ") + "\"";
        }
    }
}

