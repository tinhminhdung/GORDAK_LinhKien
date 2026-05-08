using System;

namespace Antigravity.ECommerce.Models
{
    /// <summary> Model đại diện cho Video (Youtube, v.v.) </summary>
    public class Video : BaseModel
    {
        /// <summary> Khóa chính </summary>
        public int VideoId { get; set; }
        
        /// <summary> Tiêu đề Video </summary>
        public string Title { get; set; } = string.Empty;
        
        /// <summary> ID của Video trên Youtube </summary>
        public string YoutubeId { get; set; } = string.Empty;

        /// <summary> Đường dẫn thân thiện cho Video </summary>
        public string? Slug { get; set; }
        
        /// <summary> Ảnh đại diện của Video </summary>
        public string? ThumbnailUrl { get; set; }
        
        /// <summary> ID danh mục Video (Join bảng Categories, type=3) </summary>
        public int CategoryId { get; set; }
        
        /// <summary> Thứ tự hiển thị </summary>
        public int SortOrder { get; set; }
        
        /// <summary> Trạng thái (1: Hiện, 0: Ẩn) </summary>
        public int Status { get; set; }

        /// <summary> Video nổi bật (Hiển thị sidebar/trang chủ) </summary>
        public bool IsHot { get; set; }

        /// <summary> Tiêu đề SEO Meta Title </summary>
        public string? SeoTitle { get; set; }

        /// <summary> Mô tả SEO Meta Description </summary>
        public string? SeoDescription { get; set; }

        /// <summary> Từ khóa SEO Meta Keywords </summary>
        public string? SeoKeywords { get; set; }
        
        // --- Bổ trợ (không lưu DB) ---
        /// <summary> Tên danh mục Video (dùng JOIN hiển thị) </summary>
        public string? CategoryName { get; set; }
    }
}
