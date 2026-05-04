using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Antigravity.ECommerce.Models;
using Antigravity.ECommerce.Services;

namespace Antigravity.ECommerce.Framework
{
    public class FCategory
    {
        public static Category? GetById(int id)
        {
            var prm = new SqlParameter[] { new SqlParameter("@CategoryId", id) };
            var list = BaseConnectionSql.ExecuteStoredProcedure<Category>("SP_Categories_GetById", prm);
            return list != null && list.Count > 0 ? list[0] : null;
        }

        public static Category? GetBySlug(string? slug)
        {
            var prm = new SqlParameter[] { new SqlParameter("@Slug", (object)slug ?? DBNull.Value) };
            var list = BaseConnectionSql.ExecuteStoredProcedure<Category>("SP_Categories_GetBySlug", prm);
            return list != null && list.Count > 0 ? list[0] : null;
        }

        public static List<Category> GetAll()
        {
            return SCache.GetOrSet("Category_All", () => {
                return BaseConnectionSql.ExecuteStoredProcedure<Category>("SP_Categories_GetAll", null);
            }, 60) ?? new List<Category>();
        }

        public static List<Category> Search(string? keyword, int? parentId, int? status, int? categoryType, string sortColumn, string sortOrder, int pageIndex, int pageSize)
        {
            var prm = new SqlParameter[]
            {
                new SqlParameter("@Keyword", (object?)keyword ?? DBNull.Value),
                new SqlParameter("@ParentId", (object?)parentId ?? DBNull.Value),
                new SqlParameter("@Status", (object?)status ?? DBNull.Value),
                new SqlParameter("@CategoryType", (object?)categoryType ?? DBNull.Value),
                new SqlParameter("@SortColumn", sortColumn),
                new SqlParameter("@SortOrder", sortOrder),
                new SqlParameter("@PageIndex", pageIndex),
                new SqlParameter("@PageSize", pageSize)
            };
            return BaseConnectionSql.ExecuteStoredProcedure<Category>("SP_Categories_Search", prm);
        }

        public static int Insert(Category obj)
        {
            var prm = new SqlParameter[]
            {
                new SqlParameter("@ParentId", obj.ParentId),
                new SqlParameter("@Name", obj.Name),
                new SqlParameter("@Slug", obj.Slug ?? (object)DBNull.Value),
                new SqlParameter("@Description", (object)obj.Description ?? DBNull.Value),
                new SqlParameter("@Content", (object)obj.Content ?? DBNull.Value),
                new SqlParameter("@Image", (object)obj.Image ?? DBNull.Value),
                new SqlParameter("@ImageAlt", (object)obj.ImageAlt ?? DBNull.Value),
                new SqlParameter("@Banner", (object)obj.Banner ?? DBNull.Value),
                new SqlParameter("@Icon", (object)obj.Icon ?? DBNull.Value),
                new SqlParameter("@SortOrder", obj.SortOrder),
                new SqlParameter("@Status", obj.Status),
                new SqlParameter("@SeoTitle", (object)obj.SeoTitle ?? DBNull.Value),
                new SqlParameter("@SeoDescription", (object)obj.SeoDescription ?? DBNull.Value),
                new SqlParameter("@SeoKeywords", (object)obj.SeoKeywords ?? DBNull.Value),
                new SqlParameter("@CategoryType", obj.CategoryType),
                new SqlParameter("@LinkType", obj.LinkType),
                new SqlParameter("@Url", (object)obj.Url ?? DBNull.Value),
                new SqlParameter("@Target", (object)obj.Target ?? DBNull.Value),
                new SqlParameter("@MenuPosition", (object)obj.MenuPosition ?? DBNull.Value),
                new SqlParameter("@CreatedBy", (object)obj.CreatedBy ?? DBNull.Value)
            };
            var result = BaseConnectionSql.ExecuteScalar("SP_Categories_Insert", prm);
            SCache.Remove("Category_All");
            return result != null ? Convert.ToInt32(result) : 0;
        }

        public static int Update(Category obj)
        {
            var prm = new SqlParameter[]
            {
                new SqlParameter("@CategoryId", obj.CategoryId),
                new SqlParameter("@ParentId", obj.ParentId),
                new SqlParameter("@Name", obj.Name),
                new SqlParameter("@Slug", obj.Slug ?? (object)DBNull.Value),
                new SqlParameter("@Description", (object)obj.Description ?? DBNull.Value),
                new SqlParameter("@Content", (object)obj.Content ?? DBNull.Value),
                new SqlParameter("@Image", (object)obj.Image ?? DBNull.Value),
                new SqlParameter("@ImageAlt", (object)obj.ImageAlt ?? DBNull.Value),
                new SqlParameter("@Banner", (object)obj.Banner ?? DBNull.Value),
                new SqlParameter("@Icon", (object)obj.Icon ?? DBNull.Value),
                new SqlParameter("@SortOrder", obj.SortOrder),
                new SqlParameter("@Status", obj.Status),
                new SqlParameter("@SeoTitle", (object)obj.SeoTitle ?? DBNull.Value),
                new SqlParameter("@SeoDescription", (object)obj.SeoDescription ?? DBNull.Value),
                new SqlParameter("@SeoKeywords", (object)obj.SeoKeywords ?? DBNull.Value),
                new SqlParameter("@CategoryType", obj.CategoryType),
                new SqlParameter("@LinkType", obj.LinkType),
                new SqlParameter("@Url", (object)obj.Url ?? DBNull.Value),
                new SqlParameter("@Target", (object)obj.Target ?? DBNull.Value),
                new SqlParameter("@MenuPosition", (object)obj.MenuPosition ?? DBNull.Value),
                new SqlParameter("@UpdatedBy", (object)obj.UpdatedBy ?? DBNull.Value)
            };
            var res = BaseConnectionSql.ExecuteNonQuery("SP_Categories_Update", prm);
            SCache.Remove("Category_All");
            return res;
        }

        public static int Delete(int id)
        {
            var prm = new SqlParameter[] { new SqlParameter("@CategoryId", id) };
            var res = BaseConnectionSql.ExecuteNonQuery("SP_Categories_Delete", prm);
            SCache.Remove("Category_All");
            return res;
        }
    }
}
