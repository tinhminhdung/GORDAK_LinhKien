using System;
using System.Collections.Generic;

namespace Antigravity.ECommerce.Models
{
    /// <summary> Sản phẩm trong giỏ hàng </summary>
    public class CartItem
    {
        /// <summary> ID Sản phẩm </summary>
        public int ProductId { get; set; }
        /// <summary> Tên sản phẩm </summary>
        public string ProductName { get; set; }
        /// <summary> Mã sản phẩm </summary>
        public string? ProductCode { get; set; }
        /// <summary> Thời gian bảo hành </summary>
        public string? Warranty { get; set; }
        /// <summary> Ảnh đại diện sản phẩm </summary>
        public string ProductImage { get; set; }
        /// <summary> Giá bán hiện tại </summary>
        public decimal Price { get; set; }
        /// <summary> Số lượng mua </summary>
        public int Quantity { get; set; }
        /// <summary> Thành tiền (Giá * Số lượng) </summary>
        public decimal TotalPrice => Price * Quantity;
    }

    /// <summary> View Model chứa thông tin Giỏ hàng & Thanh toán </summary>
    public class CartViewModel
    {
        /// <summary> Danh sách sản phẩm trong giỏ </summary>
        public List<CartItem> Items { get; set; } = new List<CartItem>();
        /// <summary> Tổng tiền hàng </summary>
        public decimal SubTotal { get; set; }
        /// <summary> Phí vận chuyển </summary>
        public decimal ShippingFee { get; set; } = 30000; // Default
        /// <summary> Tổng cộng (Tiền hàng + Phí vận chuyển) </summary>
        public decimal TotalAmount => (Items != null && Items.Count > 0) ? SubTotal + ShippingFee : 0;
        
        /// <summary> Họ và tên người nhận </summary>
        public string? FullName { get; set; }
        /// <summary> Số điện thoại </summary>
        public string? Phone { get; set; }
        /// <summary> Email liên hệ </summary>
        public string? Email { get; set; }
        /// <summary> Địa chỉ cụ thể </summary>
        public string? Address { get; set; }
        /// <summary> Địa chỉ đầy đủ (Đã ghép Tỉnh/Huyện/Xã) </summary>
        public string? FullAddress { get; set; }
        /// <summary> ID Tỉnh/Thành phố </summary>
        public int? ProvinceId { get; set; }
        /// <summary> ID Quận/Huyện </summary>
        public int? WardId { get; set; }
        /// <summary> Ghi chú đơn hàng </summary>
        public string? Note { get; set; }
        
        /// <summary> Có yêu cầu xuất hóa đơn VAT không </summary>
        public bool RequiresVAT { get; set; }
        
        /// <summary> Tên công ty (VAT) </summary>
        public string? VATCompanyName { get; set; }
        
        /// <summary> Mã số thuế (VAT) </summary>
        public string? VATTaxCode { get; set; }
        
        /// <summary> Địa chỉ công ty (VAT) </summary>
        public string? VATCompanyAddress { get; set; }
        
        /// <summary> Email nhận hóa đơn (VAT) </summary>
        public string? VATInvoiceEmail { get; set; }
    }
}
