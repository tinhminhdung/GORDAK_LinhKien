using System;

namespace Antigravity.ECommerce.Models
{
    public class Product : BaseModel
    {
        /// <summary> Khóa chính (Tự tăng) </summary>
        public int ProductId { get; set; }

        /// <summary> Danh sách ID danh mục (cách nhau bởi dấu phẩy) </summary>
        public string? CategoryIds { get; set; }

        /// <summary> Mã định danh sản phẩm (Duy nhất) </summary>
        public string? SKU { get; set; }

        /// <summary> Tên sản phẩm </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary> Đường dẫn thân thiện (VD: dien-thoai-iphone-15) </summary>
        public string? Slug { get; set; }

        /// <summary> Mô tả tóm tắt sản phẩm </summary>
        public string? ShortDescription { get; set; }

        /// <summary> Nội dung chi tiết sản phẩm </summary>
        public string? DetailDescription { get; set; }

        /// <summary> Giá bán hiện tại </summary>
        public decimal Price { get; set; }

        /// <summary> Giá gốc (Giá thị trường) </summary>
        public decimal OldPrice { get; set; }

        /// <summary> Giá nhập kho (Dùng để tính lợi nhuận) </summary>
        public decimal PurchasePrice { get; set; }

        /// <summary> Số lượng tồn kho </summary>
        public int Stock { get; set; }

        /// <summary> Đơn vị tính (VD: Cái, Chiếc, Kg, Bộ) </summary>
        public string? Unit { get; set; }

        /// <summary> Trọng lượng sản phẩm (Gram) </summary>
        public decimal Weight { get; set; }

        /// <summary> Ảnh đại diện chính </summary>
        public string? MainImage { get; set; }

        /// <summary> Danh sách ảnh bổ sung (cách nhau bởi dấu phẩy) </summary>
        public string? ImageGallery { get; set; }

        /// <summary> ID Video YouTube </summary>
        public string? YoutubeVideo { get; set; }

        /// <summary> Từ khóa tìm kiếm (cách nhau bởi dấu phẩy) </summary>
        public string? Tags { get; set; }

        /// <summary> Danh sách ID Sản phẩm liên quan </summary>
        public string? RelatedProducts { get; set; }

        /// <summary> Sản phẩm nổi bật (Trang chủ) </summary>
        public bool IsHot { get; set; }

        /// <summary> Sản phẩm mới về </summary>
        public bool IsNew { get; set; }

        /// <summary> Sản phẩm bán chạy </summary>
        public bool IsBestSeller { get; set; }

        /// <summary> Tổng số lượt xem khách hàng </summary>
        public int Views { get; set; }

        /// <summary> Tổng số lượng đã bán </summary>
        public int Sales { get; set; }

        /// <summary> Điểm đánh giá trung bình (0-5 sao) </summary>
        public decimal Rating { get; set; }

        /// <summary> Trạng thái (1: Đang bán, 0: Tạm ẩn) </summary>
        public int Status { get; set; }

        /// <summary> Tiêu đề SEO Meta Title </summary>
        public string? SeoTitle { get; set; }

        /// <summary> Mô tả SEO Meta Description </summary>
        public string? SeoDescription { get; set; }

        /// <summary> Từ khóa SEO Meta Keywords </summary>
        public string? SeoKeywords { get; set; }
        


        // --- Chế độ bổ trợ (Không lưu DB) ---
        /// <summary> Chuỗi tên các danh mục để hiển thị nhanh </summary>
        public string? CategoryNames { get; set; }
    }
}
