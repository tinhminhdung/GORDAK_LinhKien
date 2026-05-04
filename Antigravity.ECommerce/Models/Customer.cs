using System;

namespace Antigravity.ECommerce.Models
{
    /// <summary> Model đại diện cho Khách hàng </summary>
    public class Customer : BaseModel
    {
        /// <summary> Khóa chính </summary>
        public int CustomerId { get; set; }
        
        /// <summary> Họ và tên khách hàng </summary>
        public string FullName { get; set; } = string.Empty;
        
        /// <summary> Số điện thoại </summary>
        public string Phone { get; set; } = string.Empty;
        
        /// <summary> Email </summary>
        public string? Email { get; set; }
        
        /// <summary> Mật khẩu (đã mã hóa) </summary>
        public string? Password { get; set; }
        
        /// <summary> Ảnh đại diện </summary>
        public string? Avatar { get; set; }
        
        /// <summary> Ngày sinh </summary>
        public DateTime? DateOfBirth { get; set; }
        
        /// <summary> Giới tính (0: Khác, 1: Nam, 2: Nữ) </summary>
        public int Gender { get; set; } 
        
        /// <summary> Địa chỉ chi tiết </summary>
        public string? Address { get; set; }
        
        /// <summary> Phường/Xã </summary>
        public int? WardId { get; set; }
        
        /// <summary> Tỉnh/Thành phố </summary>
        public int? ProvinceId { get; set; }
        
        /// <summary> Hạng thành viên (1: Bạc, 2: Vàng, 3: Kim Cương) </summary>
        public int MemberRank { get; set; }
        
        /// <summary> Tổng số tiền đã mua hàng </summary>
        public decimal TotalSpent { get; set; }
        
        /// <summary> Tổng số đơn hàng đã đặt </summary>
        public int TotalOrders { get; set; }
        
        /// <summary> Điểm tích lũy </summary>
        public int Points { get; set; }
        
        /// <summary> Trạng thái (1: Hoạt động, 0: Khóa) </summary>
        public int Status { get; set; }
        
        /// <summary> Ghi chú nội bộ về khách hàng </summary>
        public string? Note { get; set; }
        


        // Mappings
        /// <summary> Tên hạng thành viên dựa trên ID </summary>
        public string MemberRankName => MemberRank switch {
            1 => "Bạc", 2 => "Vàng", 3 => "Kim Cương", _ => "Thường"
        };
    }
}
