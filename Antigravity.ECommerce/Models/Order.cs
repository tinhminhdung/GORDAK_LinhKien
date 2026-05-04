using System;

namespace Antigravity.ECommerce.Models
{
    /// <summary> Model đại diện cho Đơn hàng </summary>
    public class Order : BaseModel
    {
        /// <summary> Khóa chính </summary>
        public int OrderId { get; set; }
        
        /// <summary> Mã đơn hàng (VD: ORD12345) </summary>
        public string OrderCode { get; set; } = string.Empty;
        
        /// <summary> ID khách hàng (nếu có tài khoản) </summary>
        public int? CustomerId { get; set; }
        
        /// <summary> Tên khách hàng đặt hàng </summary>
        public string CustomerName { get; set; } = string.Empty;
        
        /// <summary> Số điện thoại nhận hàng </summary>
        public string CustomerPhone { get; set; } = string.Empty;
        
        /// <summary> Email nhận thông tin </summary>
        public string? CustomerEmail { get; set; }
        
        /// <summary> Địa chỉ nhận hàng chi tiết </summary>
        public string ShippingAddress { get; set; } = string.Empty;
        
        /// <summary> Phường/Xã </summary>
        public int? WardId { get; set; }
        
        /// <summary> Tỉnh/Thành phố </summary>
        public int? ProvinceId { get; set; }
        
        /// <summary> Tổng tiền sản phẩm </summary>
        public decimal SubTotal { get; set; }
        
        /// <summary> Phí vận chuyển </summary>
        public decimal ShippingFee { get; set; }
        
        /// <summary> Số tiền giảm giá/Voucher </summary>
        public decimal Discount { get; set; }
        
        /// <summary> Tổng tiền cuối cùng phải thanh toán </summary>
        public decimal TotalAmount { get; set; }
        
        /// <summary> Trạng thái đơn hàng (0: Mới, 1: Chờ xác nhận, 2: Đang giao, 3: Hoàn thành, 4: Hủy) </summary>
        public int OrderStatus { get; set; }      
        
        /// <summary> Phương thức thanh toán (COD, Chuyển khoản, v.v.) </summary>
        public string PaymentMethod { get; set; } = "COD";
        
        /// <summary> Trạng thái thanh toán (0: Chưa, 1: Đã thanh toán) </summary>
        public int PaymentStatus { get; set; }    
        
        /// <summary> Đơn vị vận chuyển </summary>
        public string? ShippingMethod { get; set; }
        
        /// <summary> Mã vận đơn (Tracking) </summary>
        public string? TrackingCode { get; set; }
        
        /// <summary> Ghi chú từ khách hàng </summary>
        public string? CustomerNote { get; set; }
        
        /// <summary> Ghi chú nội bộ admin </summary>
        public string? AdminNote { get; set; }
        
        /// <summary> Ngày đặt hàng </summary>
        public DateTime CreatedAt { get; set; }
        
        /// <summary> Ngày cập nhật cuối </summary>
        public DateTime? UpdatedAt { get; set; }
        
        /// <summary> Người cập nhật </summary>
        public string? UpdatedBy { get; set; }

        // Helper Properties
        /// <summary> Số lượng sản phẩm trong đơn (Không lưu DB) </summary>
        public int ItemCount { get; set; }        
        
        /// <summary> Hạng thành viên (Không lưu DB) </summary>
        public string? MemberRankName { get; set; } 

        /// <summary> Tên Tỉnh/Thành (Không lưu DB) </summary>
        public string? ProvinceName { get; set; }

        /// <summary> Tên Phường/Xã (Không lưu DB) </summary>
        public string? WardName { get; set; }
    }

    /// <summary> Model thống kê số lượng đơn hàng theo trạng thái </summary>
    public class OrderCountModel
    {
        /// <summary> Tổng tất cả đơn hàng </summary>
        public int All { get; set; }
        /// <summary> Đơn hàng mới </summary>
        public int New { get; set; }
        /// <summary> Đơn hàng đã xác nhận </summary>
        public int Confirmed { get; set; }
        /// <summary> Đơn hàng đang giao </summary>
        public int Shipping { get; set; }
        /// <summary> Đơn hàng hoàn thành </summary>
        public int Completed { get; set; }
        /// <summary> Đơn hàng đã hủy </summary>
        public int Cancelled { get; set; }
        /// <summary> Đơn hàng bị hoàn trả </summary>
        public int Returned { get; set; }
    }
}
