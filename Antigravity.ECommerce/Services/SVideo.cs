using Antigravity.ECommerce.Models;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;

namespace Antigravity.ECommerce.Services
{
    public class SVideo
    {
        public static List<Video> GetAll(bool allowCache = true)
        {
            var data = SCache.GetOrSet("AllVideos", () =>
            {
                return BaseConnectionSql.ExecuteStoredProcedure<Video>("SP_Videos_GetAll", null);
            }, 60);

            return data ?? new List<Video>();
        }

        public static Video? GetById(int id)
        {
            var prm = new SqlParameter[] { new SqlParameter("@Id", id) };
            var list = BaseConnectionSql.ExecuteStoredProcedure<Video>("SP_Videos_GetById", prm);
            return list != null && list.Count > 0 ? list[0] : null;
        }

        public static Video? GetBySlug(string slug)
        {
            var prm = new SqlParameter[] { new SqlParameter("@Slug", slug) };
            var list = BaseConnectionSql.ExecuteStoredProcedure<Video>("SP_Videos_GetBySlug", prm);
            return list != null && list.Count > 0 ? list[0] : null;
        }

        public static int Insert(Video model)
        {
            var prm = new SqlParameter[] {
                new SqlParameter("@Title", model.Title ?? ""),
                new SqlParameter("@YoutubeId", model.YoutubeId ?? ""),
                new SqlParameter("@Slug", model.Slug ?? ""),
                new SqlParameter("@ThumbnailUrl", (object?)model.ThumbnailUrl ?? DBNull.Value),
                new SqlParameter("@CategoryId", model.CategoryId),
                new SqlParameter("@SortOrder", model.SortOrder),
                new SqlParameter("@Status", model.Status),
                new SqlParameter("@SeoTitle", model.SeoTitle ?? ""),
                new SqlParameter("@SeoDescription", model.SeoDescription ?? ""),
                new SqlParameter("@SeoKeywords", model.SeoKeywords ?? ""),
                new SqlParameter("@CreatedBy", model.CreatedBy ?? "")
            };
            var result = BaseConnectionSql.ExecuteScalar("SP_Videos_Insert", prm);
            int id = result != null ? Convert.ToInt32(result) : 0;
            if (id > 0) SCache.Remove("AllVideos");
            return id;
        }

        public static int Update(Video model)
        {
            var prm = new SqlParameter[] {
                new SqlParameter("@VideoId", model.VideoId),
                new SqlParameter("@Title", model.Title ?? ""),
                new SqlParameter("@YoutubeId", model.YoutubeId ?? ""),
                new SqlParameter("@Slug", model.Slug ?? ""),
                new SqlParameter("@ThumbnailUrl", (object?)model.ThumbnailUrl ?? DBNull.Value),
                new SqlParameter("@CategoryId", model.CategoryId),
                new SqlParameter("@SortOrder", model.SortOrder),
                new SqlParameter("@Status", model.Status),
                new SqlParameter("@SeoTitle", model.SeoTitle ?? ""),
                new SqlParameter("@SeoDescription", model.SeoDescription ?? ""),
                new SqlParameter("@SeoKeywords", model.SeoKeywords ?? ""),
                new SqlParameter("@UpdatedBy", model.UpdatedBy ?? "")
            };
            var result = BaseConnectionSql.ExecuteNonQuery("SP_Videos_Update", prm);
            if (result > 0) SCache.Remove("AllVideos");
            return result;
        }

        public static int Delete(int id)
        {
            var result = BaseConnectionSql.ExecuteNonQuery("SP_Videos_Delete", new SqlParameter("@Id", id));
            if (result > 0) SCache.Remove("AllVideos");
            return result;
        }

        public static List<Video> Search(string kw, int? status, int? categoryId = null, string sort = "CreatedAt", string order = "DESC", int page = 1, int size = 20)
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
            return BaseConnectionSql.ExecuteStoredProcedure<Video>("SP_Videos_Search", prms);
        }

        public static int BulkDelete(List<int> ids)
        {
            if (ids == null || ids.Count == 0) return 0;
            string idList = string.Join(",", ids);
            string sql = $"DELETE FROM Videos WHERE VideoId IN ({idList})";
            var result = BaseConnectionSql.ExecuteNonQuery(sql, null);
            if (result > 0) SCache.Remove("AllVideos");
            return result;
        }

        public static int BulkUpdateStatus(List<int> ids, int status)
        {
            if (ids == null || ids.Count == 0) return 0;
            string idList = string.Join(",", ids);
            string sql = $"UPDATE Videos SET Status = @Status WHERE VideoId IN ({idList})";
            var result = BaseConnectionSql.ExecuteNonQuery(sql, new SqlParameter("@Status", status));
            if (result > 0) SCache.Remove("AllVideos");
            return result;
        }
    }
}

