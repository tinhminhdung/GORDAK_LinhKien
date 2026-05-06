using System;

namespace Antigravity.ECommerce.Models
{
    public class ProductOption : BaseModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        
        /// <summary>
        /// 1: Thương hiệu (Brand)
        /// 2: Tình trạng (Condition)
        /// 3: Bảo hành (Warranty)
        /// </summary>
        public int Type { get; set; }
        
        public int SortOrder { get; set; }
        public int Status { get; set; }
    }
}
