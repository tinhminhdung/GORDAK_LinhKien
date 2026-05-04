using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Antigravity.ECommerce.Models;
using Antigravity.ECommerce.Services;

namespace Antigravity.ECommerce.Framework
{
    public class FReview
    {
        public static int Insert(ProductReview model)
        {
            string sql = @"INSERT INTO ProductReviews (ProductId, CustomerId, Rating, Title, Content, IsVerifiedPurchase, Status, CustomerName, CustomerAvatar, ProductName, ProductImage)
                VALUES (@ProductId, @CustomerId, @Rating, @Title, @Content, @IsVerifiedPurchase, 1, @CustomerName, @CustomerAvatar, @ProductName, @ProductImage);
                SELECT SCOPE_IDENTITY();";
            var prms = new SqlParameter[]
            {
                new SqlParameter("@ProductId", model.ProductId),
                new SqlParameter("@CustomerId", model.CustomerId),
                new SqlParameter("@Rating", model.Rating),
                new SqlParameter("@Title", (object?)model.Title ?? DBNull.Value),
                new SqlParameter("@Content", (object?)model.Content ?? DBNull.Value),
                new SqlParameter("@IsVerifiedPurchase", model.IsVerifiedPurchase),
                new SqlParameter("@CustomerName", (object?)model.CustomerName ?? DBNull.Value),
                new SqlParameter("@CustomerAvatar", (object?)model.CustomerAvatar ?? DBNull.Value),
                new SqlParameter("@ProductName", (object?)model.ProductName ?? DBNull.Value),
                new SqlParameter("@ProductImage", (object?)model.ProductImage ?? DBNull.Value)
            };
            return Convert.ToInt32(BaseConnectionSql.ExecuteScalar(sql, prms));
        }

        public static List<ProductReview> GetByProductId(int productId, int? rating = null, int page = 1, int size = 10)
        {
            string where = "WHERE r.ProductId = @ProductId AND r.Status = 1";
            if (rating.HasValue) where += " AND r.Rating = @Rating";

            string sql = $@"SELECT r.*, COUNT(*) OVER() AS TotalCount
                FROM ProductReviews r
                {where}
                ORDER BY r.CreatedAt DESC
                OFFSET @Offset ROWS FETCH NEXT @Size ROWS ONLY";
            var prms = new List<SqlParameter>
            {
                new SqlParameter("@ProductId", productId),
                new SqlParameter("@Offset", (page - 1) * size),
                new SqlParameter("@Size", size)
            };
            if (rating.HasValue) prms.Add(new SqlParameter("@Rating", rating.Value));

            return BaseConnectionSql.Query<ProductReview>(sql, prms.ToArray());
        }

        public static List<ProductReview> GetByCustomerId(int customerId, int page = 1, int size = 20)
        {
            string sql = @"SELECT r.*, COUNT(*) OVER() AS TotalCount
                FROM ProductReviews r
                WHERE r.CustomerId = @CustomerId AND r.Status >= 0
                ORDER BY r.CreatedAt DESC
                OFFSET @Offset ROWS FETCH NEXT @Size ROWS ONLY";
            return BaseConnectionSql.Query<ProductReview>(sql,
                new SqlParameter("@CustomerId", customerId),
                new SqlParameter("@Offset", (page - 1) * size),
                new SqlParameter("@Size", size));
        }

        public static ReviewStatsModel GetProductStats(int productId)
        {
            string sql = @"SELECT 
                COUNT(*) AS TotalReviews,
                ISNULL(AVG(CAST(Rating AS DECIMAL(3,1))),0) AS AverageRating,
                SUM(CASE WHEN Rating=5 THEN 1 ELSE 0 END) AS Star5,
                SUM(CASE WHEN Rating=4 THEN 1 ELSE 0 END) AS Star4,
                SUM(CASE WHEN Rating=3 THEN 1 ELSE 0 END) AS Star3,
                SUM(CASE WHEN Rating=2 THEN 1 ELSE 0 END) AS Star2,
                SUM(CASE WHEN Rating=1 THEN 1 ELSE 0 END) AS Star1
                FROM ProductReviews WHERE ProductId = @ProductId AND Status = 1";
            return BaseConnectionSql.QuerySingle<ReviewStatsModel>(sql, new SqlParameter("@ProductId", productId))
                ?? new ReviewStatsModel();
        }

        public static List<ProductReview> SearchAdmin(string? kw, int? rating, int? status, string sort = "CreatedAt", string order = "DESC", int page = 1, int size = 20)
        {
            string where = "WHERE 1=1";
            var prms = new List<SqlParameter>();

            if (!string.IsNullOrEmpty(kw))
            {
                where += " AND (r.Content LIKE @Kw OR r.CustomerName LIKE @Kw OR r.ProductName LIKE @Kw)";
                prms.Add(new SqlParameter("@Kw", $"%{kw}%"));
            }
            if (rating.HasValue) { where += " AND r.Rating = @Rating"; prms.Add(new SqlParameter("@Rating", rating.Value)); }
            if (status.HasValue) { where += " AND r.Status = @Status"; prms.Add(new SqlParameter("@Status", status.Value)); }
            else { where += " AND r.Status >= 0"; }

            string orderBy = "r.CreatedAt DESC";
            if (sort == "Rating") orderBy = "r.Rating " + order;
            else if (sort == "ProductName") orderBy = "r.ProductName " + order;
            else if (sort == "CustomerName") orderBy = "r.CustomerName " + order;
            else if (sort == "CreatedAt") orderBy = "r.CreatedAt " + order;

            string sql = $@"SELECT r.*, COUNT(*) OVER() AS TotalCount
                FROM ProductReviews r
                {where}
                ORDER BY {orderBy}
                OFFSET @Offset ROWS FETCH NEXT @Size ROWS ONLY";
            prms.Add(new SqlParameter("@Offset", (page - 1) * size));
            prms.Add(new SqlParameter("@Size", size));

            return BaseConnectionSql.Query<ProductReview>(sql, prms.ToArray());
        }

        public static int AdminReply(int reviewId, string reply, string adminUser)
        {
            string sql = "UPDATE ProductReviews SET AdminReply = @Reply, AdminReplyAt = GETDATE(), AdminReplyBy = @By, UpdatedAt = GETDATE() WHERE ReviewId = @Id";
            return BaseConnectionSql.ExecuteNonQuery(sql,
                new SqlParameter("@Reply", reply),
                new SqlParameter("@By", adminUser),
                new SqlParameter("@Id", reviewId));
        }

        public static int ToggleStatus(int reviewId)
        {
            // Toggle between Status 1 (visible) and 0 (hidden)
            string sql = "UPDATE ProductReviews SET Status = CASE WHEN Status=1 THEN 0 ELSE 1 END, UpdatedAt = GETDATE() WHERE ReviewId = @Id";
            return BaseConnectionSql.ExecuteNonQuery(sql, new SqlParameter("@Id", reviewId));
        }

        public static int Delete(int reviewId)
        {
            string sql = "UPDATE ProductReviews SET Status = -1, UpdatedAt = GETDATE() WHERE ReviewId = @Id";
            return BaseConnectionSql.ExecuteNonQuery(sql, new SqlParameter("@Id", reviewId));
        }

        public static ProductReview? GetById(int reviewId)
        {
            return BaseConnectionSql.QuerySingle<ProductReview>(
                "SELECT * FROM ProductReviews WHERE ReviewId = @Id",
                new SqlParameter("@Id", reviewId));
        }

        public static bool HasReviewed(int customerId, int productId)
        {
            var result = BaseConnectionSql.ExecuteScalar(
                "SELECT COUNT(*) FROM ProductReviews WHERE CustomerId = @Cid AND ProductId = @Pid AND Status >= 0",
                new SqlParameter("@Cid", customerId),
                new SqlParameter("@Pid", productId));
            return Convert.ToInt32(result) > 0;
        }
    }
}
