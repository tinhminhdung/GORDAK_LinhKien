using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Antigravity.ECommerce.Models;
using Antigravity.ECommerce.Services;

namespace Antigravity.ECommerce.Framework
{
    public class FWishlist
    {
        public static int Toggle(int customerId, int productId)
        {
            var prms = new SqlParameter[]
            {
                new SqlParameter("@CustomerId", customerId),
                new SqlParameter("@ProductId", productId)
            };
            return System.Convert.ToInt32(BaseConnectionSql.ExecuteScalar("SP_Wishlists_Toggle", prms));
        }

        public static List<Wishlist> GetByCustomerId(int customerId)
        {
            var prms = new SqlParameter[] { new SqlParameter("@CustomerId", customerId) };
            return BaseConnectionSql.ExecuteStoredProcedure<Wishlist>("SP_Wishlists_GetByCustomerId", prms);
        }

        public static bool Check(int customerId, int productId)
        {
            var prms = new SqlParameter[]
            {
                new SqlParameter("@CustomerId", customerId),
                new SqlParameter("@ProductId", productId)
            };
            return System.Convert.ToInt32(BaseConnectionSql.ExecuteScalar("SP_Wishlists_Check", prms)) == 1;
        }
    }
}
