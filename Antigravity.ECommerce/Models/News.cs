using System;
using System.Collections.Generic;

namespace Antigravity.ECommerce.Models
{
    /// <summary> Model đại diện cho Bài viết / Tin tức </summary>
    public class News : BaseModel
    {
        /// <summary> Khóa chính (Tự tăng) </summary>
        public int NewsId { get; set; }

        /// <summary> ID danh mục tin tức (Join bảng Categories) </summary>
        public int CategoryId { get; set; }

        /// <summary> Tiêu đề bài viết </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary> Đường dẫn thân thiện (slug-bai-viet.html) </summary>
        public string? Slug { get; set; }

        /// <summary> Ảnh đại diện bài viết </summary>
        public string? Image { get; set; }

        /// <summary> Mô tả ngắn (Sapô) </summary>
        public string? ShortDescription { get; set; }

        /// <summary> Nội dung chi tiết bài viết (HTML) </summary>
        public string? DetailDescription { get; set; }

        /// <summary> Từ khóa liên quan (Tags) </summary>
        public string? Tags { get; set; }

        /// <summary> Số lượt xem </summary>
        public int Views { get; set; }

        /// <summary> Thứ tự sắp xếp </summary>
        public int SortOrder { get; set; }

        /// <summary> Trạng thái (1: Hiện, 0: Ẩn) </summary>
        public int Status { get; set; }

        /// <summary> Bài viết nổi bật (Hiển thị trang chủ) </summary>
        public bool IsHot { get; set; }

        /// <summary> Tiêu đề SEO Meta Title </summary>
        public string? SeoTitle { get; set; }

        /// <summary> Mô tả SEO Meta Description </summary>
        public string? SeoDescription { get; set; }

        /// <summary> Từ khóa SEO Meta Keywords </summary>
        public string? SeoKeywords { get; set; }



        // --- Bổ trợ ---
        /// <summary> Tên danh mục tin tức (Dùng để hiển thị) </summary>
        public string? CategoryName { get; set; }
    }
}
