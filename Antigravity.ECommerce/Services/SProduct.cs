using System;
using System.Collections.Generic;
using Antigravity.ECommerce.Models;
using Antigravity.ECommerce.Framework;
using Microsoft.Data.SqlClient;

namespace Antigravity.ECommerce.Services
{
    public class SProduct
    {
        public static Product? GetById(int id)
        {
            return FProduct.GetById(id);
        }

        public static List<Product> GetAll()
        {
            return Search(null, null, null, null, null, null, "Name", "ASC", 1, 1000);
        }

        public static List<Product> Search(string? keyword, string? categoryIds, int? status, bool? isHot, decimal? priceMin, decimal? priceMax, string sortColumn, string sortOrder, int pageIndex, int pageSize, DateTime? dateMin = null, DateTime? dateMax = null)
        {
            return FProduct.Search(keyword, categoryIds, status, isHot, priceMin, priceMax, sortColumn, sortOrder, pageIndex, pageSize, dateMin, dateMax);
        }

        public static int Insert(Product obj)
        {
            var result = FProduct.Insert(obj);
            if (result > 0) SCache.Remove("Products_All");
            return result;
        }

        public static int Update(Product obj)
        {
            var result = FProduct.Update(obj);
            if (result > 0) SCache.Remove("Products_All");
            return result;
        }

        public static int Delete(int id)
        {
            var result = FProduct.Delete(id);
            if (result > 0) SCache.Remove("Products_All");
            return result;
        }

        public static int ForceDelete(int id)
        {
            var prm = new SqlParameter[] { new SqlParameter("@ProductId", id) };
            var result = BaseConnectionSql.ExecuteNonQuery("SP_Products_ForceDelete", prm);
            if (result > 0) SCache.Remove("Products_All");
            return result;
        }

        public static int UpdateQuick(int id, int? status, bool? isHot)
        {
            var result = FProduct.UpdateQuick(id, status, isHot);
            if (result > 0) SCache.Remove("Products_All");
            return result;
        }
    }
}
