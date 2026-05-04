using System;
using System.ComponentModel.DataAnnotations;

namespace Antigravity.ECommerce.Models
{
    /// <summary> Model đại diện cho Tài liệu / File đính kèm (PDF, Doc, v.v.) </summary>
    public class Document : BaseModel
    {
        /// <summary> Khóa chính </summary>
        [Key]
        public int DocumentId { get; set; }
        
        /// <summary> Tiêu đề tài liệu </summary>
        [Required(ErrorMessage = "Tiêu đề không được để trống")]
        [StringLength(500)]
        public string Title { get; set; } = null!;

        /// <summary> Đường dẫn thân thiện cho Tài liệu </summary>
        public string? Slug { get; set; }
        
        /// <summary> Đường dẫn File </summary>
        [Required(ErrorMessage = "File đính kèm không được để trống")]
        [StringLength(500)]
        public string FilePath { get; set; } = null!;
        
        /// <summary> Dung lượng file (Chuỗi hiển thị) </summary>
        [StringLength(50)]
        public string? FileSize { get; set; }
        
        /// <summary> ID danh mục Tài liệu (Join bảng Categories, type=8) </summary>
        public int CategoryId { get; set; }
        
        /// <summary> Thứ tự hiển thị </summary>
        public int SortOrder { get; set; } = 0;
        
        /// <summary> Trạng thái (1: Hiện, 0: Ẩn) </summary>
        public int Status { get; set; } = 1;

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
