using System;

namespace Antigravity.ECommerce.Models
{
    public class OrderStatusHistory : BaseModel
    {
        public int HistoryId { get; set; }
        public int OrderId { get; set; }
        public int? FromStatus { get; set; }
        public int ToStatus { get; set; }
        public string? Note { get; set; }
        public string? ChangedBy { get; set; }
        public DateTime ChangedAt { get; set; }
    }
}
