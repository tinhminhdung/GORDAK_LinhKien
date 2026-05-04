using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Antigravity.ECommerce.Models;
using Antigravity.ECommerce.Services;

namespace Antigravity.ECommerce.Framework
{
    public class FProduct
    {
        public static Product? GetById(int id)
        {
            var prm = new SqlParameter[] { new SqlParameter("@ProductId", id) };
            var list = BaseConnectionSql.ExecuteStoredProcedure<Product>("SP_Products_GetById", prm);
            return list != null && list.Count > 0 ? list[0] : null;
        }

        public static Product? GetBySlug(string slug)
        {
            var prm = new SqlParameter[] { new SqlParameter("@Slug", slug) };
            var list = BaseConnectionSql.ExecuteStoredProcedure<Product>("SP_Products_GetBySlug", prm);
            return list != null && list.Count > 0 ? list[0] : null;
        }

        public static List<Product> GetBySkus(IEnumerable<string> skus)
        {
            var skuList = System.Linq.Enumerable.ToList(System.Linq.Enumerable.Where(System.Linq.Enumerable.Select(skus, s => s.Trim()), s => !string.IsNullOrEmpty(s)));
            if (skuList.Count == 0) return new List<Product>();
            
            var parameters = new List<SqlParameter>();
            var paramNames = new List<string>();
            for(int i = 0; i < skuList.Count; i++) {
                var pName = $"@sku{i}";
                paramNames.Add(pName);
                parameters.Add(new SqlParameter(pName, skuList[i]));
            }
            string sql = $"SELECT * FROM Products WHERE Status = 1 AND SKU IN ({string.Join(",", paramNames)})";
            return BaseConnectionSql.Query<Product>(sql, parameters.ToArray());
        }

        public static List<Product> Search(string? keyword, string? categoryIds, int? status, bool? isHot, decimal? priceMin, decimal? priceMax, string sortColumn, string sortOrder, int pageIndex, int pageSize, DateTime? dateMin = null, DateTime? dateMax = null)
        {
            var prm = new SqlParameter[]
            {
                new SqlParameter("@Keyword", (object)keyword ?? DBNull.Value),
                new SqlParameter("@CategoryIds", (object)categoryIds ?? DBNull.Value),
                new SqlParameter("@Status", (object)status ?? DBNull.Value),
                new SqlParameter("@IsHot", (object)isHot ?? DBNull.Value),
                new SqlParameter("@PriceMin", (object)priceMin ?? DBNull.Value),
                new SqlParameter("@PriceMax", (object)priceMax ?? DBNull.Value),
                new SqlParameter("@Ngay_Min", (object)dateMin ?? DBNull.Value),
                new SqlParameter("@Ngay_Max", (object)dateMax ?? DBNull.Value),
                new SqlParameter("@SortColumn", sortColumn),
                new SqlParameter("@SortOrder", sortOrder),
                new SqlParameter("@PageIndex", pageIndex),
                new SqlParameter("@PageSize", pageSize)
            };
            return BaseConnectionSql.ExecuteStoredProcedure<Product>("SP_Products_Search", prm);
        }

        public static int Insert(Product obj)
        {
            var prm = new SqlParameter[]
            {
                new SqlParameter("@CategoryIds", obj.CategoryIds ?? (object)DBNull.Value),
                new SqlParameter("@SKU", obj.SKU ?? (object)DBNull.Value),
                new SqlParameter("@Name", obj.Name),
                new SqlParameter("@Slug", obj.Slug ?? (object)DBNull.Value),
                new SqlParameter("@ShortDescription", (object)obj.ShortDescription ?? DBNull.Value),
                new SqlParameter("@DetailDescription", (object)obj.DetailDescription ?? DBNull.Value),
                new SqlParameter("@Price", obj.Price),
                new SqlParameter("@OldPrice", obj.OldPrice),
                new SqlParameter("@PurchasePrice", obj.PurchasePrice),
                new SqlParameter("@Stock", obj.Stock),
                new SqlParameter("@Unit", (object)obj.Unit ?? DBNull.Value),
                new SqlParameter("@Weight", obj.Weight),
                new SqlParameter("@MainImage", (object)obj.MainImage ?? DBNull.Value),
                new SqlParameter("@ImageGallery", (object)obj.ImageGallery ?? DBNull.Value),
                new SqlParameter("@YoutubeVideo", (object)obj.YoutubeVideo ?? DBNull.Value),
                new SqlParameter("@Tags", (object)obj.Tags ?? DBNull.Value),
                new SqlParameter("@RelatedProducts", (object)obj.RelatedProducts ?? DBNull.Value),
                new SqlParameter("@IsHot", obj.IsHot),
                new SqlParameter("@IsNew", obj.IsNew),
                new SqlParameter("@IsBestSeller", obj.IsBestSeller),
                new SqlParameter("@Status", obj.Status),
                new SqlParameter("@SeoTitle", (object)obj.SeoTitle ?? DBNull.Value),
                new SqlParameter("@SeoDescription", (object)obj.SeoDescription ?? DBNull.Value),
                new SqlParameter("@SeoKeywords", (object)obj.SeoKeywords ?? DBNull.Value),
                new SqlParameter("@CreatedBy", (object)obj.CreatedBy ?? DBNull.Value)
            };
            var result = BaseConnectionSql.ExecuteScalar("SP_Products_Insert", prm);
            return result != null ? Convert.ToInt32(result) : 0;
        }

        public static int Update(Product obj)
        {
            var prm = new SqlParameter[]
            {
                new SqlParameter("@ProductId", obj.ProductId),
                new SqlParameter("@CategoryIds", obj.CategoryIds ?? (object)DBNull.Value),
                new SqlParameter("@SKU", obj.SKU ?? (object)DBNull.Value),
                new SqlParameter("@Name", obj.Name),
                new SqlParameter("@Slug", obj.Slug ?? (object)DBNull.Value),
                new SqlParameter("@ShortDescription", (object)obj.ShortDescription ?? DBNull.Value),
                new SqlParameter("@DetailDescription", (object)obj.DetailDescription ?? DBNull.Value),
                new SqlParameter("@Price", obj.Price),
                new SqlParameter("@OldPrice", obj.OldPrice),
                new SqlParameter("@PurchasePrice", obj.PurchasePrice),
                new SqlParameter("@Stock", obj.Stock),
                new SqlParameter("@Unit", (object)obj.Unit ?? DBNull.Value),
                new SqlParameter("@Weight", obj.Weight),
                new SqlParameter("@MainImage", (object)obj.MainImage ?? DBNull.Value),
                new SqlParameter("@ImageGallery", (object)obj.ImageGallery ?? DBNull.Value),
                new SqlParameter("@YoutubeVideo", (object)obj.YoutubeVideo ?? DBNull.Value),
                new SqlParameter("@Tags", (object)obj.Tags ?? DBNull.Value),
                new SqlParameter("@RelatedProducts", (object)obj.RelatedProducts ?? DBNull.Value),
                new SqlParameter("@IsHot", obj.IsHot),
                new SqlParameter("@IsNew", obj.IsNew),
                new SqlParameter("@IsBestSeller", obj.IsBestSeller),
                new SqlParameter("@Status", obj.Status),
                new SqlParameter("@SeoTitle", (object)obj.SeoTitle ?? DBNull.Value),
                new SqlParameter("@SeoDescription", (object)obj.SeoDescription ?? DBNull.Value),
                new SqlParameter("@SeoKeywords", (object)obj.SeoKeywords ?? DBNull.Value),
                new SqlParameter("@UpdatedBy", (object)obj.UpdatedBy ?? DBNull.Value)
            };
            return BaseConnectionSql.ExecuteNonQuery("SP_Products_Update", prm);
        }

        public static int Delete(int id)
        {
            var prm = new SqlParameter[] { new SqlParameter("@ProductId", id) };
            return BaseConnectionSql.ExecuteNonQuery("SP_Products_Delete", prm);
        }

        public static int UpdateQuick(int id, int? status, bool? isHot)
        {
            var prm = new SqlParameter[]
            {
                new SqlParameter("@ProductId", id),
                new SqlParameter("@Status", (object)status ?? DBNull.Value),
                new SqlParameter("@IsHot", (object)isHot ?? DBNull.Value)
            };
            return BaseConnectionSql.ExecuteNonQuery("SP_Products_UpdateQuick", prm);
        }
    }
}
