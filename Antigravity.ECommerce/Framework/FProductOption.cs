using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Antigravity.ECommerce.Models;
using Antigravity.ECommerce.Services;

namespace Antigravity.ECommerce.Framework
{
    public class FProductOption
    {
        public static List<ProductOption> GetAll()
        {
            return BaseConnectionSql.ExecuteStoredProcedure<ProductOption>("SP_ProductOptions_GetAll");
        }

        public static ProductOption? GetById(int id)
        {
            var prm = new SqlParameter[] { new SqlParameter("@Id", id) };
            var list = BaseConnectionSql.ExecuteStoredProcedure<ProductOption>("SP_ProductOptions_GetById", prm);
            return list != null && list.Count > 0 ? list[0] : null;
        }

        public static int Insert(ProductOption obj)
        {
            var prm = new SqlParameter[]
            {
                new SqlParameter("@Name", obj.Name),
                new SqlParameter("@Type", obj.Type),
                new SqlParameter("@SortOrder", obj.SortOrder),
                new SqlParameter("@Status", obj.Status)
            };
            var result = BaseConnectionSql.ExecuteScalar("SP_ProductOptions_Insert", prm);
            return result != null ? Convert.ToInt32(result) : 0;
        }

        public static int Update(ProductOption obj)
        {
            var prm = new SqlParameter[]
            {
                new SqlParameter("@Id", obj.Id),
                new SqlParameter("@Name", obj.Name),
                new SqlParameter("@Type", obj.Type),
                new SqlParameter("@SortOrder", obj.SortOrder),
                new SqlParameter("@Status", obj.Status)
            };
            return BaseConnectionSql.ExecuteNonQuery("SP_ProductOptions_Update", prm);
        }

        public static int Delete(int id)
        {
            var prm = new SqlParameter[] { new SqlParameter("@Id", id) };
            return BaseConnectionSql.ExecuteNonQuery("SP_ProductOptions_Delete", prm);
        }
    }
}
