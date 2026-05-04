using System;

namespace Antigravity.ECommerce.Models
{
    /// <summary> Model đại diện cho 1 ảnh trong Album (bảng GalleryImages) </summary>
    public class GalleryImage
    {
        /// <summary> Khóa chính </summary>
        public int ImageId { get; set; }
        
        /// <summary> ID Album chứa ảnh này </summary>
        public int GalleryId { get; set; }
        
        /// <summary> Đường dẫn ảnh </summary>
        public string ImagePath { get; set; } = string.Empty;
        
        /// <summary> Mô tả ảnh (Caption) </summary>
        public string? Caption { get; set; }
        
        /// <summary> Thứ tự sắp xếp </summary>
        public int SortOrder { get; set; }
        
        /// <summary> Ngày thêm ảnh </summary>
        public DateTime CreatedAt { get; set; }
    }
}
