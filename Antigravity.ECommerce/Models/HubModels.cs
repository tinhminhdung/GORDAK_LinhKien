using System;
using System.Collections.Generic;

namespace Antigravity.ECommerce.Models
{
    /// <summary> Model một mã sản phẩm nhận từ HUB (Luồng A) </summary>
    public class HubSyncItem
    {
        public string code { get; set; } = string.Empty;
        public int stock { get; set; }
        public long price { get; set; }
        public string? unit { get; set; }
        public string? expected { get; set; }
    }

    /// <summary> Payload danh sách sản phẩm nhận từ HUB (Luồng A) </summary>
    public class HubSyncPayload
    {
        public List<HubSyncItem> items { get; set; } = new List<HubSyncItem>();
    }

    /// <summary> Dữ liệu gửi form "Báo cho tôi" từ Client (Luồng B) </summary>
    public class HubNotifyRequest
    {
        public string name { get; set; } = string.Empty;
        public string phone { get; set; } = string.Empty;
        public string address { get; set; } = string.Empty;
        public string sku { get; set; } = string.Empty;
        public int qty { get; set; } = 1;
        public string? note { get; set; }
        /// <summary> Ô ẩn Honeypot chống Bot spam </summary>
        public string? hp_address_code { get; set; }
    }

    /// <summary> Kết quả gửi sang HUB </summary>
    public class HubSendResult
    {
        public bool Success { get; set; }
        public int StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? ErrorDetail { get; set; }
    }

    /// <summary> Hằng số 8 câu phương thức giao hàng chuẩn mực theo quy định HUB (29/10/2026) </summary>
    public static class HubDeliveryConstants
    {
        public static readonly string[] StandardMethods = new string[]
        {
            "Chuẩn bị hàng để khách qua lấy và thanh toán tại cửa hàng",
            "Ship hàng cho khách về địa chỉ",
            "Gửi qua dịch vụ chuyển phát (ViettelPost) - Ship COD – chuyển phát nhanh",
            "Gửi qua dịch vụ chuyển phát (ViettelPost) - Ship COD – chuyển phát thường (tiết kiệm)",
            "Gửi qua dịch vụ chuyển phát (ViettelPost) - Chuyển khoản trước – chuyển phát nhanh",
            "Gửi qua dịch vụ chuyển phát (ViettelPost) - Chuyển khoản trước – chuyển phát thường (tiết kiệm)",
            "Gửi hàng ra xe - Chuyển khoản trước và gửi hàng ra xe",
            "Gửi hàng ra xe - Gửi hàng ra và thu tiền tại nhà xe"
        };

        public const string DefaultMethod = "Chuẩn bị hàng để khách qua lấy và thanh toán tại cửa hàng";
    }
}

