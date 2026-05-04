using System;

namespace Antigravity.ECommerce.Models
{
    public class Advertising : BaseModel
    {
        /// <summary> Khóa chính (Tự tăng) </summary>
        public int AdvertisingId { get; set; }

        /// <summary> Tiêu đề hiển thị của Banner/Quảng cáo </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary> Đường dẫn File ảnh </summary>
        public string? Image { get; set; }

        /// <summary> Link YouTube hoặc đường dẫn Video </summary>
        public string? VideoUrl { get; set; }

        /// <summary> Đường dẫn chuyển hướng khi click </summary>
        public string? Link { get; set; }

        /// <summary> Nội dung mô tả chi tiết (dùng cho các khối nội dung trang chủ) </summary>
        public string? Description { get; set; }

        /// <summary> Vị trí hiển thị (VD: Home_Slide, Popup, Sidebar_Left) </summary>
        public string? Position { get; set; }

        /// <summary> Thứ tự ưu tiên hiển thị </summary>
        public int SortOrder { get; set; }

        /// <summary> Trạng thái (1: Hiển thị, 0: Ẩn) </summary>
        public int Status { get; set; }

        /// <summary> Cách mở link: _self (trang hiện tại) hoặc _blank (tab mới) </summary>
        public string? Target { get; set; } = "_self";

        /// <summary> Thời gian bắt đầu hiển thị </summary>
        public DateTime? StartDate { get; set; }

        /// <summary> Thời gian kết thúc hiển thị </summary>
        public DateTime? EndDate { get; set; }


    }
}
