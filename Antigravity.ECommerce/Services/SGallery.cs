using Antigravity.ECommerce.Models;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;

namespace Antigravity.ECommerce.Services
{
    public class SGallery
    {
        public static List<Gallery> GetAll(bool allowCache = true)
        {
            var data = SCache.GetOrSet("AllGalleries", () =>
            {
                return BaseConnectionSql.ExecuteStoredProcedure<Gallery>("SP_Galleries_GetAll", null);
            }, 60);
            return data ?? new List<Gallery>();
        }

        public static Gallery? GetById(int id)
        {
            var prm = new SqlParameter[] { new SqlParameter("@Id", id) };
            var list = BaseConnectionSql.ExecuteStoredProcedure<Gallery>("SP_Galleries_GetById", prm);
            if (list != null && list.Count > 0)
            {
                var gallery = list[0];
                // Load áº£nh chi tiáº¿t tá»« báº£ng GalleryImages
                gallery.ImageList = SGalleryImage.GetByGalleryId(gallery.GalleryId);
                gallery.ImageCount = gallery.ImageList.Count;
                return gallery;
            }
            return null;
        }

        public static Gallery? GetBySlug(string slug)
        {
            var prm = new SqlParameter[] { new SqlParameter("@Slug", slug) };
            var list = BaseConnectionSql.ExecuteStoredProcedure<Gallery>("SP_Galleries_GetBySlug", prm);
            if (list != null && list.Count > 0)
            {
                var gallery = list[0];
                gallery.ImageList = SGalleryImage.GetByGalleryId(gallery.GalleryId);
                gallery.ImageCount = gallery.ImageList.Count;
                return gallery;
            }
            return null;
        }

        public static int Insert(Gallery model)
        {
            var prm = new SqlParameter[] {
                new SqlParameter("@AlbumName", model.AlbumName ?? ""),
                new SqlParameter("@Slug", model.Slug ?? ""),
                new SqlParameter("@CoverImage", (object?)model.CoverImage ?? DBNull.Value),
                new SqlParameter("@Description", (object?)model.Description ?? DBNull.Value),
                new SqlParameter("@Images", (object?)model.Images ?? DBNull.Value),
                new SqlParameter("@CategoryId", model.CategoryId),
                new SqlParameter("@SortOrder", model.SortOrder),
                new SqlParameter("@Status", model.Status),
                new SqlParameter("@SeoTitle", model.SeoTitle ?? ""),
                new SqlParameter("@SeoDescription", model.SeoDescription ?? ""),
                new SqlParameter("@SeoKeywords", model.SeoKeywords ?? ""),
                new SqlParameter("@CreatedBy", model.CreatedBy ?? "")
            };
            var result = BaseConnectionSql.ExecuteScalar("SP_Galleries_Insert", prm);
            int id = result != null ? Convert.ToInt32(result) : 0;
            if (id > 0) SCache.Remove("AllGalleries");
            return id;
        }

        public static int Update(Gallery model)
        {
            var prm = new SqlParameter[] {
                new SqlParameter("@GalleryId", model.GalleryId),
                new SqlParameter("@AlbumName", model.AlbumName ?? ""),
                new SqlParameter("@Slug", model.Slug ?? ""),
                new SqlParameter("@CoverImage", (object?)model.CoverImage ?? DBNull.Value),
                new SqlParameter("@Description", (object?)model.Description ?? DBNull.Value),
                new SqlParameter("@Images", (object?)model.Images ?? DBNull.Value),
                new SqlParameter("@CategoryId", model.CategoryId),
                new SqlParameter("@SortOrder", model.SortOrder),
                new SqlParameter("@Status", model.Status),
                new SqlParameter("@SeoTitle", model.SeoTitle ?? ""),
                new SqlParameter("@SeoDescription", model.SeoDescription ?? ""),
                new SqlParameter("@SeoKeywords", model.SeoKeywords ?? ""),
                new SqlParameter("@UpdatedBy", model.UpdatedBy ?? "")
            };
            var result = BaseConnectionSql.ExecuteNonQuery("SP_Galleries_Update", prm);
            if (result > 0) SCache.Remove("AllGalleries");
            return result;
        }

        public static int Delete(int id)
        {
            // GalleryImages sáº½ tá»± xÃ³a theo CASCADE
            var result = BaseConnectionSql.ExecuteNonQuery("SP_Galleries_Delete", new SqlParameter("@Id", id));
            if (result > 0) SCache.Remove("AllGalleries");
            return result;
        }

        public static List<Gallery> Search(string kw, int? status, int? categoryId = null, string sort = "CreatedAt", string order = "DESC", int page = 1, int size = 20)
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
            return BaseConnectionSql.ExecuteStoredProcedure<Gallery>("SP_Galleries_Search", prms);
        }

        public static int BulkDelete(List<int> ids)
        {
            if (ids == null || ids.Count == 0) return 0;
            string idList = string.Join(",", ids);
            string sql = $"DELETE FROM Galleries WHERE GalleryId IN ({idList})";
            var result = BaseConnectionSql.ExecuteNonQuery(sql, null);
            if (result > 0) SCache.Remove("AllGalleries");
            return result;
        }

        public static int BulkUpdateStatus(List<int> ids, int status)
        {
            if (ids == null || ids.Count == 0) return 0;
            string idList = string.Join(",", ids);
            string sql = $"UPDATE Galleries SET Status = @Status WHERE GalleryId IN ({idList})";
            var result = BaseConnectionSql.ExecuteNonQuery(sql, new SqlParameter("@Status", status));
            if (result > 0) SCache.Remove("AllGalleries");
            return result;
        }
    }
}

