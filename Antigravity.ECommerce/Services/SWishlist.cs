using System.Collections.Generic;
using Antigravity.ECommerce.Models;
using Antigravity.ECommerce.Framework;

namespace Antigravity.ECommerce.Services
{
    public class SWishlist
    {
        public static int Toggle(int customerId, int productId)
        {
            return FWishlist.Toggle(customerId, productId);
        }

        public static List<Wishlist> GetByCustomerId(int customerId)
        {
            return FWishlist.GetByCustomerId(customerId);
        }

        public static bool Check(int customerId, int productId)
        {
            return FWishlist.Check(customerId, productId);
        }
    }
}
