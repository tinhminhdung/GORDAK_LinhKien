using System;

namespace Antigravity.ECommerce.Models
{
    public class Wishlist
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public int ProductId { get; set; }
        public DateTime CreatedAt { get; set; }
        
        // Helper properties joined from Product
        public string? ProductName { get; set; }
        public string? ProductImage { get; set; }
        public decimal Price { get; set; }
        public string? Slug { get; set; }
    }
}
