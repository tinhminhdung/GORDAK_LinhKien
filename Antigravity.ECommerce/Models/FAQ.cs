using System;

namespace Antigravity.ECommerce.Models
{
    /// <summary> Model đại diện cho Câu hỏi thường gặp (FAQ) </summary>
    public class FAQ : BaseModel
    {
        /// <summary> Khóa chính </summary>
        public int FAQId { get; set; }
        
        /// <summary> Câu hỏi </summary>
        public string Question { get; set; } = string.Empty;

        /// <summary> Đường dẫn thân thiện cho FAQ (nếu có trang chi tiết) </summary>
        public string? Slug { get; set; }
        
        /// <summary> Câu trả lời (Hỗ trợ HTML) </summary>
        public string Answer { get; set; } = string.Empty;
        
        /// <summary> ID danh mục Hỏi đáp (Join bảng Categories, type=7) </summary>
        public int CategoryId { get; set; }
        
        /// <summary> Thứ tự hiển thị </summary>
        public int SortOrder { get; set; }
        
        /// <summary> Trạng thái </summary>
        public int Status { get; set; }

        /// <summary> Lượt xem </summary>
        public int Views { get; set; }

        /// <summary> Tiêu đề SEO Meta Title </summary>
        public string? SeoTitle { get; set; }

        /// <summary> Mô tả SEO Meta Description </summary>
        public string? SeoDescription { get; set; }

        /// <summary> Từ khóa SEO Meta Keywords </summary>
        public string? SeoKeywords { get; set; }
        
        // --- Bổ trợ ---
        /// <summary> Tên danh mục (dùng JOIN hiển thị) </summary>
        public string? CategoryName { get; set; }
    }
}
