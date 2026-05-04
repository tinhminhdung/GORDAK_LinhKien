using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Antigravity.ECommerce.Models;
using Antigravity.ECommerce.Framework;

namespace Antigravity.ECommerce.Services
{
    /// <summary> Dá»‹ch vá»¥ quáº£n lÃ½ Tin tá»©c / BÃ i viáº¿t </summary>
    public class SNews
    {
        public static List<News> GetAll(int top = 0)
        {
            return SCache.GetOrSet("News_GetAll_" + top, () => {
                var prm = new SqlParameter[] { new SqlParameter("@Top", top) };
                return BaseConnectionSql.ExecuteStoredProcedure<News>("SP_News_GetAll", prm);
            }, 60);
        }

        public static News? GetById(int id)
        {
            var prm = new SqlParameter[] { new SqlParameter("@Id", id) };
            var list = BaseConnectionSql.ExecuteStoredProcedure<News>("SP_News_GetById", prm);
            return list != null && list.Count > 0 ? list[0] : null;
        }

        public static News? GetBySlug(string slug)
        {
            var prm = new SqlParameter[] { new SqlParameter("@Slug", slug) };
            var list = BaseConnectionSql.ExecuteStoredProcedure<News>("SP_News_GetBySlug", prm);
            return list != null && list.Count > 0 ? list[0] : null;
        }

        public static List<News> Search(string kw, int? catId, int? status, string sort, string order, int pageIndex, int pageSize)
        {
            var prm = new SqlParameter[] { 
                new SqlParameter("@Keyword", (object?)kw ?? DBNull.Value),
                new SqlParameter("@CategoryId", (object?)catId ?? DBNull.Value),
                new SqlParameter("@Status", (object?)status ?? DBNull.Value),
                new SqlParameter("@SortColumn", sort ?? "CreatedAt"),
                new SqlParameter("@SortOrder", order ?? "DESC"),
                new SqlParameter("@PageIndex", pageIndex),
                new SqlParameter("@PageSize", pageSize)
            };
            
            return BaseConnectionSql.ExecuteStoredProcedure<News>("SP_News_Search", prm);
        }

        public static int Insert(News obj)
        {
            var prm = GetParameters(obj);
            var result = BaseConnectionSql.ExecuteScalar("SP_News_Insert", prm);
            int id = result != null ? Convert.ToInt32(result) : 0;
            if (id > 0) SCache.Remove("News_All");
            return id;
        }

        public static int Update(News obj)
        {
            var prm = GetParameters(obj);
            var result = BaseConnectionSql.ExecuteNonQuery("SP_News_Update", prm);
            if (result > 0) SCache.Remove("News_All");
            return result;
        }

        public static int Delete(int id)
        {
            var prm = new SqlParameter[] { new SqlParameter("@Id", id) };
            var result = BaseConnectionSql.ExecuteNonQuery("SP_News_Delete", prm);
            if (result > 0) SCache.Remove("News_All");
            return result;
        }

        public static int BulkDelete(List<int> ids)
        {
            if (ids == null || ids.Count == 0) return 0;
            int count = 0;
            foreach (var id in ids)
            {
                count += Delete(id);
            }
            return count;
        }

        public static int BulkUpdateStatus(List<int> ids, int status)
        {
            if (ids == null || ids.Count == 0) return 0;
            int count = 0;
            foreach (var id in ids)
            {
                count += BaseConnectionSql.ExecuteNonQuery("UPDATE News SET Status = @Status WHERE NewsId = @Id", 
                    new SqlParameter("@Status", status), new SqlParameter("@Id", id));
            }
            if (count > 0) SCache.Remove("News_All");
            return count;
        }

        private static SqlParameter[] GetParameters(News obj)
        {
            return new SqlParameter[] {
                new SqlParameter("@NewsId", obj.NewsId),
                new SqlParameter("@CategoryId", obj.CategoryId),
                new SqlParameter("@Title", obj.Title ?? ""),
                new SqlParameter("@Slug", (object?)obj.Slug ?? DBNull.Value),
                new SqlParameter("@Image", (object?)obj.Image ?? DBNull.Value),
                new SqlParameter("@ShortDescription", (object?)obj.ShortDescription ?? DBNull.Value),
                new SqlParameter("@DetailDescription", (object?)obj.DetailDescription ?? DBNull.Value),
                new SqlParameter("@Tags", (object?)obj.Tags ?? DBNull.Value),
                new SqlParameter("@SortOrder", obj.SortOrder),
                new SqlParameter("@Status", obj.Status),
                new SqlParameter("@IsHot", obj.IsHot),
                new SqlParameter("@SeoTitle", (object?)obj.SeoTitle ?? DBNull.Value),
                new SqlParameter("@SeoDescription", (object?)obj.SeoDescription ?? DBNull.Value),
                new SqlParameter("@SeoKeywords", (object?)obj.SeoKeywords ?? DBNull.Value),
                new SqlParameter("@CreatedBy", (object?)obj.CreatedBy ?? DBNull.Value),
                new SqlParameter("@UpdatedBy", (object?)obj.UpdatedBy ?? DBNull.Value)
            };
        }
    }
}

