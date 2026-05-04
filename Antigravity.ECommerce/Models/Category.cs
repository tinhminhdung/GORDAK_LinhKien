using System;

namespace Antigravity.ECommerce.Models
{
    public class Category : BaseModel
    {
        /// <summary> Khóa chính (Tự tăng) </summary>
        public int CategoryId { get; set; }

        /// <summary> ID danh mục cha (0 = Cấp gốc) </summary>
        public int ParentId { get; set; }

        /// <summary> Tên danh mục hiển thị </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary> Đường dẫn thân thiện (VD: dien-thoai-iphone) </summary>
        public string? Slug { get; set; }

        /// <summary> Mô tả ngắn (Mô tả danh mục) </summary>
        public string? Description { get; set; }

        /// <summary> Nội dung chi tiết (Dùng cho Trang nội dung - Content Page) </summary>
        public string? Content { get; set; }

        /// <summary> Ảnh đại diện của danh mục </summary>
        public string? Image { get; set; }

        /// <summary> Chuỗi mô tả ảnh cho SEO </summary>
        public string? ImageAlt { get; set; }

        /// <summary> Ảnh bìa (Banner) phía trên cùng trang </summary>
        public string? Banner { get; set; }

        /// <summary> Icon hiển thị (Class FontAwesome hoặc đường dẫn ảnh) </summary>
        public string? Icon { get; set; }

        /// <summary> Thứ tự sắp xếp (số nhỏ hiển thị trước) </summary>
        public int SortOrder { get; set; }

        /// <summary> Trạng thái (1: Hiện, 0: Ẩn) </summary>
        public int Status { get; set; }

        /// <summary> Tiêu đề SEO Meta Title </summary>
        public string? SeoTitle { get; set; }

        /// <summary> Mô tả SEO Meta Description </summary>
        public string? SeoDescription { get; set; }

        /// <summary> Từ khóa SEO Meta Keywords </summary>
        public string? SeoKeywords { get; set; }
        
        /// <summary> 
        /// Loại dữ liệu: 1: Sản phẩm, 2: Tin tức, 3: Video, 4: Trang nội dung, 5: Link ngoài, 6: Thư viện ảnh, 7: Hỏi đáp, 8: Tài liệu
        /// </summary>
        public int CategoryType { get; set; } 

        /// <summary> Tên loại danh mục (dễ đọc thay vì mã số) </summary>
        public string? CategoryTypeName { get; set; }

        /// <summary> Kiểu liên kết: 1: Hệ thống (theo Slug), 2: Link tùy chỉnh </summary>
        public int LinkType { get; set; } 

        /// <summary> URL tùy chỉnh nếu LinkType = 2 </summary>
        public string? Url { get; set; }

        /// <summary> Cách mở trang: _self (tại chỗ) hoặc _blank (tab mới) </summary>
        public string? Target { get; set; } = "_self";

        /// <summary> Vị trí menu hiển thị (VD: Header, Footer, Sidebar) </summary>
        public string? MenuPosition { get; set; } 
        

        
        // --- Chế độ bổ trợ (Không lưu DB) ---

        /// <summary> Tên của danh mục cha </summary>
        public string? ParentName { get; set; }

        /// <summary> Số lượng sản phẩm/bài viết/tin tức thuộc danh mục này </summary>
        public int ItemCount { get; set; }

        /// <summary> Thuộc tính cũ (Dùng để tương thích ngược) </summary>
        public int ProductCount { get => ItemCount; set => ItemCount = value; }

        /// <summary> Danh sách danh mục con cấp dưới </summary>
        public List<Category> Children { get; set; } = new List<Category>();
    }
}
