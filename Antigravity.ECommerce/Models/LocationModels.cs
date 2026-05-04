using System;

namespace Antigravity.ECommerce.Models
{
    public class Province
    {
        public int ProvinceId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Type { get; set; }
    }

    public class Ward
    {
        public int WardId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Type { get; set; }
        public int ProvinceId { get; set; }
        public string? ProvinceName { get; set; }
    }
}
