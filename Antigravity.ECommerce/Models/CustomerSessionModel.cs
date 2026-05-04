namespace Antigravity.ECommerce.Models
{
    public class CustomerSessionModel
    {
        public int CustomerId { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Avatar { get; set; }
        public int MemberRank { get; set; }
        public string? MemberRankName { get; set; }
    }
}
