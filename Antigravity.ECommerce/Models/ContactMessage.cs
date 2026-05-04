using System;

namespace Antigravity.ECommerce.Models
{
    public class ContactMessage : BaseModel
    {
        public int ContactId { get; set; }
        
        /// <summary> Họ và tên khách hàng </summary>
        public string FullName { get; set; } = string.Empty;
        
        /// <summary> Email liên hệ </summary>
        public string Email { get; set; } = string.Empty;
        
        /// <summary> Số điện thoại </summary>
        public string? Phone { get; set; }
        
        /// <summary> Tiêu đề liên hệ </summary>
        public string? Subject { get; set; }
        
        /// <summary> Nội dung tin nhắn </summary>
        public string Message { get; set; } = string.Empty;
        
        /// <summary> Trạng thái (0: Mới, 1: Đã đọc, 2: Đã trả lời, 3: Đã xử lý) </summary>
        public int Status { get; set; } = 0;
        
        /// <summary> Đánh dấu tin nhắn quan trọng </summary>
        public bool IsStarred { get; set; } = false;
        
        /// <summary> Ghi chú của Admin </summary>
        public string? AdminNote { get; set; }
        
        /// <summary> Thời điểm Admin xem tin nhắn </summary>
        public DateTime? ReadAt { get; set; }

        // Mở rộng từ BaseModel: CreatedAt (Thời điểm gửi), CreatedBy, UpdatedAt, UpdatedBy
    }
}
