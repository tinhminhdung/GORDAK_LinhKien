using Antigravity.ECommerce.Models;
using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;

namespace Antigravity.ECommerce.Services
{
    /// <summary> Service quản lý ảnh chi tiết trong Album (bảng GalleryImages) </summary>
    public class SGalleryImage
    {
        public static List<GalleryImage> GetByGalleryId(int galleryId)
        {
            var prm = new SqlParameter[] { new SqlParameter("@GalleryId", galleryId) };
            return BaseConnectionSql.ExecuteStoredProcedure<GalleryImage>("SP_GalleryImages_GetByGalleryId", prm) ?? new List<GalleryImage>();
        }

        public static int Insert(GalleryImage model)
        {
            var prm = new SqlParameter[] {
                new SqlParameter("@GalleryId", model.GalleryId),
                new SqlParameter("@ImagePath", model.ImagePath ?? ""),
                new SqlParameter("@Caption", (object?)model.Caption ?? DBNull.Value),
                new SqlParameter("@SortOrder", model.SortOrder)
            };
            var result = BaseConnectionSql.ExecuteScalar("SP_GalleryImages_Insert", prm);
            return result != null ? Convert.ToInt32(result) : 0;
        }

        public static int Delete(int imageId)
        {
            var prm = new SqlParameter[] { new SqlParameter("@ImageId", imageId) };
            return BaseConnectionSql.ExecuteNonQuery("SP_GalleryImages_Delete", prm);
        }

        /// <summary> Thêm nhiều ảnh cùng lúc vào album </summary>
        public static int BulkInsert(int galleryId, List<string> imagePaths)
        {
            if (imagePaths == null || imagePaths.Count == 0) return 0;
            
            // Lấy SortOrder hiện tại lớn nhất
            var existing = GetByGalleryId(galleryId);
            int maxSort = existing.Count > 0 ? existing.Max(x => x.SortOrder) + 1 : 0;
            
            int count = 0;
            foreach (var path in imagePaths)
            {
                if (string.IsNullOrWhiteSpace(path)) continue;
                Insert(new GalleryImage { GalleryId = galleryId, ImagePath = path.Trim(), SortOrder = maxSort });
                maxSort++;
                count++;
            }
            return count;
        }

        /// <summary> Cập nhật thứ tự ảnh (Drag & Drop) </summary>
        public static void Reorder(int galleryId, List<int> imageIds)
        {
            if (imageIds == null) return;
            for (int i = 0; i < imageIds.Count; i++)
            {
                var prm = new SqlParameter[] {
                    new SqlParameter("@ImageId", imageIds[i]),
                    new SqlParameter("@SortOrder", i)
                };
                BaseConnectionSql.ExecuteNonQuery("SP_GalleryImages_UpdateSort", prm);
            }
        }

        /// <summary> Cập nhật caption của ảnh </summary>
        public static int UpdateCaption(int imageId, string? caption)
        {
            string sql = "UPDATE GalleryImages SET Caption = @Caption WHERE ImageId = @ImageId";
            return BaseConnectionSql.ExecuteNonQuery(sql, 
                new SqlParameter("@ImageId", imageId),
                new SqlParameter("@Caption", (object?)caption ?? DBNull.Value));
        }
    }
}
