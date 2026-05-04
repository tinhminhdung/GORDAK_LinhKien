using Antigravity.ECommerce.Models;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System;

namespace Antigravity.ECommerce.Services
{
    public class SFAQ
    {
        public static List<FAQ> GetAll(bool allowCache = true)
        {
            var data = SCache.GetOrSet("AllFAQs", () =>
            {
                return BaseConnectionSql.ExecuteStoredProcedure<FAQ>("SP_FAQs_GetAll", null);
            }, 60);

            return data ?? new List<FAQ>();
        }

        public static FAQ? GetById(int id)
        {
            var prm = new SqlParameter[] { new SqlParameter("@Id", id) };
            var list = BaseConnectionSql.ExecuteStoredProcedure<FAQ>("SP_FAQs_GetById", prm);
            return list != null && list.Count > 0 ? list[0] : null;
        }

        public static int Insert(FAQ model)
        {
            var prm = new SqlParameter[] {
                new SqlParameter("@Question", model.Question ?? ""),
                new SqlParameter("@Slug", model.Slug ?? ""),
                new SqlParameter("@Answer", model.Answer ?? ""),
                new SqlParameter("@CategoryId", model.CategoryId),
                new SqlParameter("@SortOrder", model.SortOrder),
                new SqlParameter("@Status", model.Status),
                new SqlParameter("@SeoTitle", model.SeoTitle ?? ""),
                new SqlParameter("@SeoDescription", model.SeoDescription ?? ""),
                new SqlParameter("@SeoKeywords", model.SeoKeywords ?? ""),
                new SqlParameter("@CreatedBy", model.CreatedBy ?? "")
            };
            var result = BaseConnectionSql.ExecuteScalar("SP_FAQs_Insert", prm);
            int id = result != null ? Convert.ToInt32(result) : 0;
            if (id > 0) SCache.Remove("AllFAQs");
            return id;
        }

        public static int Update(FAQ model)
        {
            var prm = new SqlParameter[] {
                new SqlParameter("@FAQId", model.FAQId),
                new SqlParameter("@Question", model.Question ?? ""),
                new SqlParameter("@Answer", model.Answer ?? ""),
                new SqlParameter("@Slug", model.Slug ?? ""),
                new SqlParameter("@CategoryId", model.CategoryId),
                new SqlParameter("@SortOrder", model.SortOrder),
                new SqlParameter("@Status", model.Status),
                new SqlParameter("@SeoTitle", model.SeoTitle ?? ""),
                new SqlParameter("@SeoDescription", model.SeoDescription ?? ""),
                new SqlParameter("@SeoKeywords", model.SeoKeywords ?? ""),
                new SqlParameter("@UpdatedBy", model.UpdatedBy ?? "")
            };
            var result = BaseConnectionSql.ExecuteNonQuery("SP_FAQs_Update", prm);
            if (result > 0) SCache.Remove("AllFAQs");
            return result;
        }

        public static int Delete(int id)
        {
            var result = BaseConnectionSql.ExecuteNonQuery("SP_FAQs_Delete", new SqlParameter("@Id", id));
            if (result > 0) SCache.Remove("AllFAQs");
            return result;
        }

        public static List<FAQ> Search(string kw, int? status, int? categoryId = null, string sort = "CreatedAt", string order = "DESC", int page = 1, int size = 20)
        {
            var prms = new SqlParameter[] {
                new SqlParameter("@Keyword", (object?)kw ?? DBNull.Value),
                new SqlParameter("@Status", (object?)status ?? DBNull.Value),
                new SqlParameter("@CategoryId", (object?)categoryId ?? DBNull.Value),
                new SqlParameter("@SortColumn", sort ?? "CreatedAt"),
                new SqlParameter("@SortOrder", order ?? "DESC"),
                new SqlParameter("@PageIndex", page),
                new SqlParameter("@PageSize", size)
            };
            return BaseConnectionSql.ExecuteStoredProcedure<FAQ>("SP_FAQs_Search", prms);
        }

        public static int BulkDelete(List<int> ids)
        {
            if (ids == null || ids.Count == 0) return 0;
            string idList = string.Join(",", ids);
            string sql = $"DELETE FROM FAQs WHERE FAQId IN ({idList})";
            var result = BaseConnectionSql.ExecuteNonQuery(sql, null);
            if (result > 0) SCache.Remove("AllFAQs");
            return result;
        }

        public static int BulkUpdateStatus(List<int> ids, int status)
        {
            if (ids == null || ids.Count == 0) return 0;
            string idList = string.Join(",", ids);
            string sql = $"UPDATE FAQs SET Status = @Status WHERE FAQId IN ({idList})";
            var result = BaseConnectionSql.ExecuteNonQuery(sql, new SqlParameter("@Status", status));
            if (result > 0) SCache.Remove("AllFAQs");
            return result;
        }
    }
}

