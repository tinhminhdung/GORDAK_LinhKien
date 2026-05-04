using System;

namespace Antigravity.ECommerce.Models
{
    public class AdminLog
    {
        public int LogId { get; set; }
        public string Username { get; set; }
        public string Action { get; set; }
        public string Module { get; set; }
        public string Description { get; set; }
        public string IpAddress { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
