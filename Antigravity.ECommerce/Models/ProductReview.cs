using System;

namespace Antigravity.ECommerce.Models
{
    /// <summary> Model đại diện cho một Đánh giá / Bình luận sản phẩm </summary>
    public class ProductReview
    {
        /// <summary> ID Đánh giá (Khóa chính) </summary>
        public int ReviewId { get; set; }
        /// <summary> ID Sản phẩm được đánh giá </summary>
        public int ProductId { get; set; }
        /// <summary> ID Khách hàng (User đã login) </summary>
        public int CustomerId { get; set; }
        /// <summary> Số sao đánh giá (1-5) </summary>
        public int Rating { get; set; }
        /// <summary> Tiêu đề bài đánh giá </summary>
        public string? Title { get; set; }
        /// <summary> Nội dung đánh giá chi tiết </summary>
        public string? Content { get; set; }
        /// <summary> Nội dung phản hồi từ Quản trị viên </summary>
        public string? AdminReply { get; set; }
        /// <summary> Thời gian Quản trị viên phản hồi </summary>
        public DateTime? AdminReplyAt { get; set; }
        /// <summary> Người phản hồi (Username Admin) </summary>
        public string? AdminReplyBy { get; set; }
        /// <summary> Cờ đánh dấu đã mua hàng thật sự </summary>
        public bool IsVerifiedPurchase { get; set; }
        /// <summary> Số lượt người khác bấm Hữu ích </summary>
        public int HelpfulCount { get; set; }
        /// <summary> Trạng thái (1: Hiện, 0: Ẩn, -1: Đã xóa) </summary>
        public int Status { get; set; } = 1; 
        /// <summary> Ngày tạo đánh giá </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        /// <summary> Ngày cập nhật đánh giá </summary>
        public DateTime? UpdatedAt { get; set; }

        // --- Denormalized / helper ---
        /// <summary> Tên hiển thị của khách hàng </summary>
        public string? CustomerName { get; set; }
        /// <summary> URL Avatar của khách hàng </summary>
        public string? CustomerAvatar { get; set; }
        /// <summary> Tên sản phẩm </summary>
        public string? ProductName { get; set; }
        /// <summary> Ảnh sản phẩm </summary>
        public string? ProductImage { get; set; }
        /// <summary> Tổng số bản ghi (Phục vụ phân trang) </summary>
        public int TotalCount { get; set; }

        /// <summary> Helper hiển thị icon sao dựa trên Rating </summary>
        public string RatingStars
        {
            get
            {
                string stars = "";
                for (int i = 1; i <= 5; i++)
                    stars += i <= Rating ? "★" : "☆";
                return stars;
            }
        }
    }

    /// <summary> Thống kê số liệu đánh giá của một sản phẩm </summary>
    public class ReviewStatsModel
    {
        /// <summary> Tổng số lượng đánh giá </summary>
        public int TotalReviews { get; set; }
        /// <summary> Điểm trung bình (vd: 4.5) </summary>
        public decimal AverageRating { get; set; }
        /// <summary> Số lượng đánh giá 5 sao </summary>
        public int Star5 { get; set; }
        /// <summary> Số lượng đánh giá 4 sao </summary>
        public int Star4 { get; set; }
        /// <summary> Số lượng đánh giá 3 sao </summary>
        public int Star3 { get; set; }
        /// <summary> Số lượng đánh giá 2 sao </summary>
        public int Star2 { get; set; }
        /// <summary> Số lượng đánh giá 1 sao </summary>
        public int Star1 { get; set; }
    }
}
