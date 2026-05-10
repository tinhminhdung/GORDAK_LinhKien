using System;
using System.Collections.Generic;

namespace Antigravity.ECommerce.Models
{
    /// <summary> Model đại diện cho Album ảnh / Thư viện ảnh </summary>
    public class Gallery : BaseModel
    {
        /// <summary> Khóa chính </summary>
        public int GalleryId { get; set; }
        
        /// <summary> Tên Album ảnh </summary>
        public string AlbumName { get; set; } = string.Empty;

        /// <summary> Đường dẫn thân thiện cho Album </summary>
        public string? Slug { get; set; }
        
        /// <summary> Mô tả Album </summary>
        public string? Description { get; set; }
        
        /// <summary> Ảnh bìa Album </summary>
        public string? CoverImage { get; set; }
        
        /// <summary> Danh sách ảnh dạng chuỗi (legacy, dùng cho tương thích ngược) </summary>
        public string? Images { get; set; }
        
        /// <summary> ID danh mục Thư viện ảnh (Join bảng Categories, type=6) </summary>
        public int CategoryId { get; set; }
        
        /// <summary> Thứ tự hiển thị </summary>
        public int SortOrder { get; set; }
        
        /// <summary> Trạng thái (1: Hiện, 0: Ẩn) </summary>
        public int Status { get; set; }

        /// <summary> Lượt xem </summary>
        public int Views { get; set; }

        /// <summary> Tiêu đề SEO Meta Title </summary>
        public string? SeoTitle { get; set; }

        /// <summary> Mô tả SEO Meta Description </summary>
        public string? SeoDescription { get; set; }

        /// <summary> Từ khóa SEO Meta Keywords </summary>
        public string? SeoKeywords { get; set; }
        
        // --- Bổ trợ (không lưu DB trực tiếp) ---
        
        /// <summary> Tên danh mục (dùng JOIN hiển thị) </summary>
        public string? CategoryName { get; set; }
        
        /// <summary> Số lượng ảnh trong album (từ bảng GalleryImages) </summary>
        public int ImageCount { get; set; }
        
        /// <summary> Danh sách ảnh chi tiết (load từ bảng GalleryImages) </summary>
        public List<GalleryImage> ImageList { get; set; } = new List<GalleryImage>();
    }
}
