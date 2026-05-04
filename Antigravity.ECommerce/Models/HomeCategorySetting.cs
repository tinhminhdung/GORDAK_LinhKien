namespace Antigravity.ECommerce.Models
{
    /// <summary> Model cho cấu hình danh mục hiển thị trang chủ (bảng riêng, không ảnh hưởng Categories) </summary>
    public class HomeCategorySetting
    {
        /// <summary> ID danh mục </summary>
        public int CategoryId { get; set; }
        /// <summary> Thứ tự hiển thị ngoài trang chủ </summary>
        public int SortOrder { get; set; }
        /// <summary> Số lượng sản phẩm muốn hiển thị </summary>
        public int ProductCount { get; set; } = 4;
        /// <summary> Trạng thái hiển thị (true: Bật, false: Tắt) </summary>
        public bool IsActive { get; set; } = true;
        
        // --- JOIN fields ---
        /// <summary> Tên danh mục (Từ bảng Categories) </summary>
        public string Name { get; set; } = "";
        /// <summary> Đường dẫn tĩnh (Slug) </summary>
        public string? Slug { get; set; }
        /// <summary> Ảnh đại diện danh mục </summary>
        public string? Image { get; set; }
    }
}
