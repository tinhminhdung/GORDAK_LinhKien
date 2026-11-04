using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Antigravity.ECommerce.Models;
using Antigravity.ECommerce.Framework;

namespace Antigravity.ECommerce.Services
{
    /// <summary>
    /// Dịch vụ gửi đơn hàng và đăng ký "Báo cho tôi" sang máy chủ HUB (Luồng B)
    /// Đảm bảo chuẩn hóa 4 điểm mới nhất ngày 29/10/2026 và tuân thủ quy tắc an toàn bảo mật.
    /// </summary>
    public static class HubSender
    {
        private static readonly HttpClient _httpClient;

        static HubSender()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;
            _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };
        }

        /// <summary>
        /// Gửi chuỗi JSON sang HUB kèm chữ ký số HMAC-SHA256
        /// </summary>
        public static async Task<HubSendResult> SendJsonAsync(string json, string url, string key)
        {
            if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(key))
            {
                return new HubSendResult { Success = false, StatusCode = 0, Message = "Chưa cấu hình Hub:OrderUrl hoặc Hub:OrderKey" };
            }

            string ts = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
            string sig;
            byte[] bodyBytes = Encoding.UTF8.GetBytes(json);

            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key)))
            {
                byte[] pre = Encoding.UTF8.GetBytes(ts + ".");
                byte[] all = new byte[pre.Length + bodyBytes.Length];
                Buffer.BlockCopy(pre, 0, all, 0, pre.Length);
                Buffer.BlockCopy(bodyBytes, 0, all, pre.Length, bodyBytes.Length);
                sig = Convert.ToHexString(hmac.ComputeHash(all)).ToLowerInvariant();
            }

            try
            {
                using var req = new HttpRequestMessage(HttpMethod.Post, url);
                req.Content = new StringContent(json, Encoding.UTF8, "application/json");
                req.Headers.Add("X-Hub-Timestamp", ts);
                req.Headers.Add("X-Hub-Signature", sig);

                using var res = await _httpClient.SendAsync(req).ConfigureAwait(false);
                int statusCode = (int)res.StatusCode;
                string resBody = await res.Content.ReadAsStringAsync().ConfigureAwait(false);

                bool isOk = statusCode == 200 && (resBody.Contains("\"ok\":true", StringComparison.OrdinalIgnoreCase) || resBody.Contains("\"ok\": true", StringComparison.OrdinalIgnoreCase));

                return new HubSendResult
                {
                    Success = isOk,
                    StatusCode = statusCode,
                    Message = isOk ? "Gửi sang HUB thành công!" : (!string.IsNullOrEmpty(resBody) ? resBody : ("HTTP " + statusCode)),
                    ErrorDetail = isOk ? null : resBody
                };
            }
            catch (Exception ex)
            {
                return new HubSendResult { Success = false, StatusCode = 0, Message = "Lỗi kết nối HUB: " + ex.Message, ErrorDetail = ex.ToString() };
            }
        }

        /// <summary>
        /// Gửi đơn hàng sang HUB (kind: "order") theo chuẩn 29/10/2026
        /// </summary>
        public static async Task<HubSendResult> SendOrderAsync(int orderId, IConfiguration cfg)
        {
            try
            {
                var order = FOrder.GetById(orderId);
                if (order == null) return new HubSendResult { Success = false, StatusCode = 404, Message = "Không tìm thấy đơn hàng #" + orderId };

                var orderItems = FOrder.GetItemsByOrderId(orderId);
                string source = SSetting.GetValue("Hub_Source");
                if (string.IsNullOrEmpty(source)) source = cfg["HubSettings:Source"] ?? "gordak";

                string url = SSetting.GetValue("Hub_OrderUrl");
                if (string.IsNullOrEmpty(url)) url = cfg["HubSettings:OrderUrl"] ?? "https://pos.nhatminhesc.com/api/web/orders";

                string key = SSetting.GetValue("Hub_OrderKey");
                if (string.IsNullOrEmpty(key)) key = cfg["HubSettings:OrderKey"] ?? "";

                if (string.IsNullOrEmpty(key))
                {
                    FOrder.UpdateHubStatus(orderId, false, "Chưa cấu hình HubSettings:OrderKey (trong Admin hoặc appsettings.json)");
                    return new HubSendResult { Success = false, StatusCode = 400, Message = "Chưa cấu hình Hub_OrderKey trong Cấu hình hệ thống Admin hoặc appsettings.json" };
                }

                // 1. Chuẩn bị danh sách sản phẩm (ĐƠN GIÁ LUÔN LÀ GIÁ CHƯA VAT)
                var itemsList = new List<object>();
                long totalWithoutVat = 0;

                foreach (var item in orderItems)
                {
                    var product = FProduct.GetById(item.ProductId);
                    string sku = !string.IsNullOrEmpty(item.ProductSKU) ? item.ProductSKU : (product?.SKU ?? "SKU-" + item.ProductId);
                    string unit = product?.Unit ?? "Chiếc";

                    // Đơn giá chưa VAT: Ưu tiên OldPrice nếu có, ngược lại tính từ Price / 1.1
                    long priceWithoutVat = 0;
                    if (product != null && product.OldPrice > 0)
                    {
                        priceWithoutVat = (long)Math.Round(product.OldPrice);
                    }
                    else if (item.Price > 0)
                    {
                        priceWithoutVat = (long)Math.Round(item.Price / 1.1m);
                    }

                    int qty = item.Quantity > 0 ? item.Quantity : 1;
                    itemsList.Add(new
                    {
                        sku = sku,
                        name = item.ProductName,
                        qty = qty,
                        price = priceWithoutVat,
                        unit = unit
                    });

                    totalWithoutVat += (priceWithoutVat * qty);
                }

                // 2. Bóc tách Phương thức giao hàng (1 trong 8 câu chuẩn)
                string deliveryMethod = !string.IsNullOrEmpty(order.DeliveryMethod) ? order.DeliveryMethod : HubDeliveryConstants.DefaultMethod;
                string deliveryNote = order.DeliveryNote ?? "";
                bool deliveryChosen = order.DeliveryChosen;

                // 3. Chuẩn hóa số điện thoại
                string cleanPhone = (order.CustomerPhone ?? "").Replace(" ", "").Replace(".", "").Replace("-", "");
                if (cleanPhone.StartsWith("+84")) cleanPhone = "0" + cleanPhone.Substring(3);
                if (cleanPhone.StartsWith("84") && cleanPhone.Length == 11) cleanPhone = "0" + cleanPhone.Substring(2);

                // 4. Xây dựng Payload JSON
                var payload = new Dictionary<string, object>
                {
                    { "kind", "order" },
                    { "source", source },
                    { "orderCode", !string.IsNullOrEmpty(order.OrderCode) ? order.OrderCode : ("ORD" + order.OrderId) },
                    { "createdAt", order.CreatedAt.ToString("yyyy-MM-ddTHH:mm:sszzz") },
                    {
                        "customer", new Dictionary<string, object>
                        {
                            { "name", order.CustomerName ?? "" },
                            { "phone", cleanPhone },
                            { "email", order.CustomerEmail ?? "" },
                            { "address", order.ShippingAddress ?? "" }
                        }
                    },
                    {
                        "delivery", new Dictionary<string, object>
                        {
                            { "method", deliveryMethod },
                            { "note", deliveryNote },
                            { "chosen", deliveryChosen },
                            { "fee", 0 }
                        }
                    },
                    {
                        "payment", new Dictionary<string, object>
                        {
                            { "method", "" } // Bỏ trống theo chuẩn ngày 29/10/2026
                        }
                    },
                    { "items", itemsList },
                    { "total", totalWithoutVat },
                    { "note", order.CustomerNote ?? "" }
                };

                // Nếu có yêu cầu hóa đơn công ty -> thêm company (kèm company.email)
                if (order.RequiresVAT && (!string.IsNullOrEmpty(order.VATCompanyName) || !string.IsNullOrEmpty(order.VATTaxCode)))
                {
                    string compEmail = (order.VATInvoiceEmail ?? "").Trim();
                    // Phòng vệ: HUB quy định email tối đa 100 ký tự và là định dạng email
                    // Nếu khách nhập/paste nhầm địa chỉ hoặc chuỗi dài > 100 ký tự -> làm sạch để tránh lỗi HTTP 400
                    if (compEmail.Length > 100 || !compEmail.Contains("@") || compEmail.Contains(" ") || compEmail.Contains(","))
                    {
                        compEmail = "";
                    }

                    payload["company"] = new Dictionary<string, object>
                    {
                        { "name", order.VATCompanyName ?? "" },
                        { "taxCode", order.VATTaxCode ?? "" },
                        { "address", order.VATCompanyAddress ?? "" },
                        { "email", compEmail }
                    };
                }

                string json = JsonSerializer.Serialize(payload);
                var sendRes = await SendJsonAsync(json, url, key).ConfigureAwait(false);

                // Cập nhật trạng thái gửi đơn vào database
                FOrder.UpdateHubStatus(orderId, sendRes.Success, sendRes.Success ? null : sendRes.Message);
                return sendRes;
            }
            catch (Exception ex)
            {
                FOrder.UpdateHubStatus(orderId, false, ex.Message);
                return new HubSendResult { Success = false, StatusCode = 0, Message = "Lỗi xử lý gửi đơn: " + ex.Message };
            }
        }

        /// <summary>
        /// Gửi đăng ký "Báo cho tôi" sang HUB (kind: "notify")
        /// </summary>
        public static async Task<HubSendResult> SendNotifyAsync(HubNotifyRequest req, IConfiguration cfg)
        {
            try
            {
                // 1. Kiểm tra Honeypot chống bot spam
                if (!string.IsNullOrEmpty(req.hp_address_code))
                {
                    return new HubSendResult { Success = true, StatusCode = 200, Message = "Cảm ơn quý khách! Nhật Minh sẽ liên hệ ngay khi có hàng." };
                }

                // 2. Đọc thông tin sản phẩm chuẩn từ CSDL theo SKU
                var product = FProduct.GetBySku(req.sku);
                if (product == null)
                {
                    return new HubSendResult { Success = false, StatusCode = 404, Message = "Không tìm thấy sản phẩm trong hệ thống với mã " + req.sku };
                }

                string source = SSetting.GetValue("Hub_Source");
                if (string.IsNullOrEmpty(source)) source = cfg["HubSettings:Source"] ?? "gordak";

                string url = SSetting.GetValue("Hub_NotifyUrl");
                if (string.IsNullOrEmpty(url)) url = SSetting.GetValue("Hub_OrderUrl");
                if (string.IsNullOrEmpty(url)) url = cfg["HubSettings:NotifyUrl"] ?? cfg["HubSettings:OrderUrl"] ?? "https://pos.nhatminhesc.com/api/web/orders";

                string key = SSetting.GetValue("Hub_NotifyKey");
                if (string.IsNullOrEmpty(key)) key = SSetting.GetValue("Hub_OrderKey");
                if (string.IsNullOrEmpty(key)) key = cfg["HubSettings:NotifyKey"] ?? cfg["HubSettings:OrderKey"] ?? "";

                // Đơn giá chưa VAT
                long priceWithoutVat = (product.OldPrice > 0) ? (long)Math.Round(product.OldPrice) : (long)Math.Round(product.Price / 1.1m);
                string unit = !string.IsNullOrEmpty(product.Unit) ? product.Unit : "Chiếc";
                string? expectedDateStr = product.ExpectedDate.HasValue ? product.ExpectedDate.Value.ToString("yyyy-MM-dd") : null;

                // Chuẩn hóa SĐT
                string cleanPhone = (req.phone ?? "").Replace(" ", "").Replace(".", "").Replace("-", "");
                if (cleanPhone.StartsWith("+84")) cleanPhone = "0" + cleanPhone.Substring(3);
                if (cleanPhone.StartsWith("84") && cleanPhone.Length == 11) cleanPhone = "0" + cleanPhone.Substring(2);

                string orderCode = "NTF" + DateTime.Now.ToString("yyMMddHHmmss") + new Random().Next(100, 999);

                var payload = new Dictionary<string, object>
                {
                    { "kind", "notify" },
                    { "source", source },
                    { "orderCode", orderCode },
                    { "createdAt", DateTimeOffset.Now.ToString("yyyy-MM-ddTHH:mm:sszzz") },
                    {
                        "customer", new Dictionary<string, object>
                        {
                            { "name", req.name.Trim() },
                            { "phone", cleanPhone },
                            { "address", req.address.Trim() }
                        }
                    },
                    {
                        "items", new List<object>
                        {
                            new
                            {
                                sku = product.SKU,
                                name = product.Name,
                                qty = req.qty > 0 ? req.qty : 1,
                                price = priceWithoutVat,
                                unit = unit,
                                expected = expectedDateStr
                            }
                        }
                    },
                    { "note", req.note?.Trim() ?? "" }
                };

                string json = JsonSerializer.Serialize(payload);

                // 3. Tạm lưu vào hàng đợi HubNotifyQueue qua Stored Procedure
                try
                {
                    var prms = new SqlParameter[] { new SqlParameter("@OrderCode", orderCode), new SqlParameter("@Json", json) };
                    BaseConnectionSql.ExecuteNonQuery("SP_HubNotifyQueue_Enqueue", prms);
                }
                catch { }

                // 4. Gửi sang HUB
                var sendRes = await SendJsonAsync(json, url, key).ConfigureAwait(false);

                // 5. QUY TẮC AN TOÀN BẢO MẬT THÔNG TIN KHÁCH HÀNG:
                // Nếu gửi thành công (200 OK) -> XÓA NGAY LẬP TỨC khỏi HubNotifyQueue
                if (sendRes.Success)
                {
                    try
                    {
                        var prmsDel = new SqlParameter[] { new SqlParameter("@OrderCode", orderCode) };
                        BaseConnectionSql.ExecuteNonQuery("SP_HubNotifyQueue_Delete", prmsDel);
                    }
                    catch { }
                }
                else
                {
                    try
                    {
                        var prmsErr = new SqlParameter[] { new SqlParameter("@OrderCode", orderCode), new SqlParameter("@HubLastError", sendRes.Message ?? "") };
                        BaseConnectionSql.ExecuteNonQuery("SP_HubNotifyQueue_UpdateError", prmsErr);
                    }
                    catch { }
                }

                return sendRes;
            }
            catch (Exception ex)
            {
                return new HubSendResult { Success = false, StatusCode = 0, Message = "Lỗi gửi thông báo: " + ex.Message };
            }
        }
    }
}
