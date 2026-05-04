using System.Collections.Generic;

namespace Antigravity.ECommerce.Models
{
    public class TinhThanh
    {
        public int Ma_Tinh { get; set; }
        public string Ten_Tinh { get; set; } = string.Empty;
        public string? Loai { get; set; }
    }

    public class PhuongXa
    {
        public int Ma_Xa { get; set; }
        public string Ten_Xa { get; set; } = string.Empty;
        public string? Loai { get; set; }
        public int Ma_Tinh { get; set; }
    }
}
