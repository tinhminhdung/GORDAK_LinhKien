using System;

namespace Antigravity.ECommerce.Models
{
    /// <summary> Model cơ sở chứa các thông tin dùng chung (Metadata) </summary>
    public class BaseModel
    {
        public string? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        
        public int TotalCount { get; set; }
    }
}
