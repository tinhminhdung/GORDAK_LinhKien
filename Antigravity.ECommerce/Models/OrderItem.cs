using System;

namespace Antigravity.ECommerce.Models
{
    /// <summary> Model đại diện cho chi tiết một dòng sản phẩm trong Đơn hàng </summary>
    public class OrderItem : BaseModel
    {
        /// <summary> Khóa chính </summary>
        public int OrderItemId { get; set; }
        
        /// <summary> ID đơn hàng cha </summary>
        public int OrderId { get; set; }
        
        /// <summary> ID sản phẩm </summary>
        public int ProductId { get; set; }
        
        /// <summary> Tên sản phẩm tại thời điểm mua </summary>
        public string ProductName { get; set; } = string.Empty;
        
        /// <summary> Mã SKU sản phẩm </summary>
        public string? ProductSKU { get; set; }
        
        /// <summary> Ảnh sản phẩm tại thời điểm mua </summary>
        public string? ProductImage { get; set; }
        
        /// <summary> Đơn giá tại thời điểm mua </summary>
        public decimal Price { get; set; }
        
        /// <summary> Số lượng mua </summary>
        public int Quantity { get; set; }
        
        /// <summary> Thành tiền (Price * Quantity) </summary>
        public decimal TotalPrice { get; set; }
    }
}
