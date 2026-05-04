using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Antigravity.ECommerce.Models;
using Antigravity.ECommerce.Framework;

namespace Antigravity.ECommerce.Services
{
    public class SHomeCategorySetting
    {
        public static List<HomeCategorySetting> GetAll()
        {
            return SCache.GetOrSet("HomeCategorySettings_All", () => {
                return BaseConnectionSql.ExecuteStoredProcedure<HomeCategorySetting>("SP_HomeCategorySettings_GetAll", null);
            }, 60) ?? new List<HomeCategorySetting>();
        }

        public static int Toggle(int categoryId)
        {
            var prm = new SqlParameter[] { new SqlParameter("@CategoryId", categoryId) };
            var result = BaseConnectionSql.ExecuteNonQuery("SP_HomeCategorySettings_Toggle", prm);
            if (result > 0) 
            {
                SCache.Remove("HomeCategorySettings_All");
                SCache.Remove("Home_HomeCategories");
                SCache.Remove($"Home_CategoryProducts_{categoryId}");
            }
            return result;
        }

        public static int Update(int categoryId, int sortOrder, int productCount)
        {
            var prm = new SqlParameter[] {
                new SqlParameter("@CategoryId", categoryId),
                new SqlParameter("@SortOrder", sortOrder),
                new SqlParameter("@ProductCount", productCount)
            };
            var result = BaseConnectionSql.ExecuteNonQuery("SP_HomeCategorySettings_Update", prm);
            if (result > 0) 
            {
                SCache.Remove("HomeCategorySettings_All");
                SCache.Remove("Home_HomeCategories");
                SCache.Remove($"Home_CategoryProducts_{categoryId}");
            }
            return result;
        }
    }
}
