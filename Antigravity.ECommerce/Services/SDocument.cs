using Antigravity.ECommerce.Models;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System;

namespace Antigravity.ECommerce.Services
{
    public class SDocument
    {
        public static List<Document> GetAll(bool allowCache = true)
        {
            var data = SCache.GetOrSet("AllDocuments", () =>
            {
                return BaseConnectionSql.ExecuteStoredProcedure<Document>("SP_Documents_GetAll", null);
            }, 60);
            return data ?? new List<Document>();
        }

        public static Document? GetById(int id)
        {
            var prm = new SqlParameter[] { new SqlParameter("@Id", id) };
            var list = BaseConnectionSql.ExecuteStoredProcedure<Document>("SP_Documents_GetById", prm);
            return list != null && list.Count > 0 ? list[0] : null;
        }

        public static int Insert(Document model)
        {
            var prm = new SqlParameter[] {
                new SqlParameter("@Title", model.Title ?? ""),
                new SqlParameter("@FilePath", model.FilePath ?? ""),
                new SqlParameter("@FileSize", (object?)model.FileSize ?? DBNull.Value),
                new SqlParameter("@Slug", model.Slug ?? ""),
                new SqlParameter("@CategoryId", model.CategoryId),
                new SqlParameter("@SortOrder", model.SortOrder),
                new SqlParameter("@Status", model.Status),
                new SqlParameter("@SeoTitle", model.SeoTitle ?? ""),
                new SqlParameter("@SeoDescription", model.SeoDescription ?? ""),
                new SqlParameter("@SeoKeywords", model.SeoKeywords ?? ""),
                new SqlParameter("@CreatedBy", model.CreatedBy ?? "")
            };
            var result = BaseConnectionSql.ExecuteScalar("SP_Documents_Insert", prm);
            int id = result != null ? Convert.ToInt32(result) : 0;
            if (id > 0) SCache.Remove("AllDocuments");
            return id;
        }

        public static int Update(Document model)
        {
            var prm = new SqlParameter[] {
                new SqlParameter("@DocumentId", model.DocumentId),
                new SqlParameter("@Title", model.Title ?? ""),
                new SqlParameter("@FilePath", model.FilePath ?? ""),
                new SqlParameter("@FileSize", (object?)model.FileSize ?? DBNull.Value),
                new SqlParameter("@Slug", model.Slug ?? ""),
                new SqlParameter("@CategoryId", model.CategoryId),
                new SqlParameter("@SortOrder", model.SortOrder),
                new SqlParameter("@Status", model.Status),
                new SqlParameter("@SeoTitle", model.SeoTitle ?? ""),
                new SqlParameter("@SeoDescription", model.SeoDescription ?? ""),
                new SqlParameter("@SeoKeywords", model.SeoKeywords ?? ""),
                new SqlParameter("@UpdatedBy", model.UpdatedBy ?? "")
            };
            var result = BaseConnectionSql.ExecuteNonQuery("SP_Documents_Update", prm);
            if (result > 0) SCache.Remove("AllDocuments");
            return result;
        }

        public static bool Delete(int id)
        {
            var result = BaseConnectionSql.ExecuteNonQuery("SP_Documents_Delete", new SqlParameter("@Id", id));
            if (result > 0) SCache.Remove("AllDocuments");
            return result > 0;
        }

        public static List<Document> Search(string kw, int? status, int? categoryId = null, string sort = "CreatedAt", string order = "DESC", int page = 1, int size = 20)
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
            return BaseConnectionSql.ExecuteStoredProcedure<Document>("SP_Documents_Search", prms);
        }

        public static int BulkDelete(List<int> ids)
        {
            if (ids == null || ids.Count == 0) return 0;
            string idList = string.Join(",", ids);
            string sql = $"DELETE FROM Documents WHERE DocumentId IN ({idList})";
            var result = BaseConnectionSql.ExecuteNonQuery(sql, null);
            if (result > 0) SCache.Remove("AllDocuments");
            return result;
        }

        public static int BulkUpdateStatus(List<int> ids, int status)
        {
            if (ids == null || ids.Count == 0) return 0;
            string idList = string.Join(",", ids);
            string sql = $"UPDATE Documents SET Status = @Status WHERE DocumentId IN ({idList})";
            var result = BaseConnectionSql.ExecuteNonQuery(sql, new SqlParameter("@Status", status));
            if (result > 0) SCache.Remove("AllDocuments");
            return result;
        }

        public static void SeedSampleData()
        {
            // Removed for brevity unless required. Assuming old code might had this for testing.
        }
    }
}

