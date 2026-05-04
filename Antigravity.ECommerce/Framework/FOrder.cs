using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Antigravity.ECommerce.Models;
using Antigravity.ECommerce.Services;

namespace Antigravity.ECommerce.Framework
{
    public class FOrder
    {
        public static int Insert(Order model)
        {
            var prms = new SqlParameter[]
            {
                new SqlParameter("@CustomerId", (object?)model.CustomerId ?? DBNull.Value),
                new SqlParameter("@CustomerName", model.CustomerName ?? string.Empty),
                new SqlParameter("@CustomerPhone", model.CustomerPhone ?? string.Empty),
                new SqlParameter("@CustomerEmail", (object?)model.CustomerEmail ?? DBNull.Value),
                new SqlParameter("@ShippingAddress", model.ShippingAddress ?? string.Empty),
                new SqlParameter("@WardId", (object?)model.WardId ?? DBNull.Value),
                new SqlParameter("@ProvinceId", (object?)model.ProvinceId ?? DBNull.Value),
                new SqlParameter("@SubTotal", model.SubTotal),
                new SqlParameter("@ShippingFee", model.ShippingFee),
                new SqlParameter("@Discount", model.Discount),
                new SqlParameter("@TotalAmount", model.TotalAmount),
                new SqlParameter("@OrderStatus", model.OrderStatus),
                new SqlParameter("@PaymentMethod", model.PaymentMethod ?? "COD"),
                new SqlParameter("@PaymentStatus", model.PaymentStatus),
                new SqlParameter("@CustomerNote", (object?)model.CustomerNote ?? DBNull.Value),
                new SqlParameter("@CreatedBy", (object?)model.UpdatedBy ?? DBNull.Value)
            };
            return Convert.ToInt32(BaseConnectionSql.ExecuteScalar("SP_Orders_Insert", prms));
        }

        public static int Update(Order model)
        {
            var prms = new SqlParameter[]
            {
                new SqlParameter("@OrderId", model.OrderId),
                new SqlParameter("@CustomerName", model.CustomerName ?? string.Empty),
                new SqlParameter("@CustomerPhone", model.CustomerPhone ?? string.Empty),
                new SqlParameter("@ShippingAddress", model.ShippingAddress ?? string.Empty),
                new SqlParameter("@WardId", (object?)model.WardId ?? DBNull.Value),
                new SqlParameter("@ProvinceId", (object?)model.ProvinceId ?? DBNull.Value),
                new SqlParameter("@PaymentStatus", model.PaymentStatus),
                new SqlParameter("@ShippingMethod", (object?)model.ShippingMethod ?? DBNull.Value),
                new SqlParameter("@TrackingCode", (object?)model.TrackingCode ?? DBNull.Value),
                new SqlParameter("@AdminNote", (object?)model.AdminNote ?? DBNull.Value),
                new SqlParameter("@UpdatedBy", (object?)model.UpdatedBy ?? DBNull.Value)
            };
            return BaseConnectionSql.ExecuteNonQuery("SP_Orders_Update", prms);
        }

        public static int UpdateStatus(int orderId, int newStatus, string? trackingCode, string? note, string? updatedBy)
        {
            var prms = new SqlParameter[]
            {
                new SqlParameter("@OrderId", orderId),
                new SqlParameter("@NewStatus", newStatus),
                new SqlParameter("@TrackingCode", (object?)trackingCode ?? DBNull.Value),
                new SqlParameter("@Note", (object?)note ?? DBNull.Value),
                new SqlParameter("@UpdatedBy", (object?)updatedBy ?? DBNull.Value)
            };
            return BaseConnectionSql.ExecuteNonQuery("SP_Orders_UpdateStatus", prms);
        }

        public static List<Order> Search(string? kw, int? status, int? paymentStatus, int? provinceId, int? wardId, DateTime? dateMin, DateTime? dateMax, 
            string sort, string order, int page, int size)
        {
            var prms = new SqlParameter[]
            {
                new SqlParameter("@Keyword", (object?)kw ?? DBNull.Value),
                new SqlParameter("@OrderStatus", (object?)status ?? DBNull.Value),
                new SqlParameter("@PaymentStatus", (object?)paymentStatus ?? DBNull.Value),
                new SqlParameter("@ProvinceId", (object?)provinceId ?? DBNull.Value),
                new SqlParameter("@WardId", (object?)wardId ?? DBNull.Value),
                new SqlParameter("@Ngay_Min", (object?)dateMin ?? DBNull.Value),
                new SqlParameter("@Ngay_Max", (object?)dateMax ?? DBNull.Value),
                new SqlParameter("@SortColumn", sort ?? "CreatedAt"),
                new SqlParameter("@SortOrder", order ?? "DESC"),
                new SqlParameter("@PageIndex", page),
                new SqlParameter("@PageSize", size)
            };
            return BaseConnectionSql.ExecuteStoredProcedure<Order>("SP_Orders_Search", prms);
        }

        public static Order? GetById(int orderId)
        {
            var prms = new SqlParameter[] { new SqlParameter("@OrderId", orderId) };
            var list = BaseConnectionSql.ExecuteStoredProcedure<Order>("SP_Orders_GetById", prms);
            if (list != null && list.Count > 0) return list[0];
            return null;
        }

        public static int Delete(int orderId)
        {
            var prms = new SqlParameter[] { new SqlParameter("@OrderId", orderId) };
            return BaseConnectionSql.ExecuteNonQuery("SP_Orders_Delete", prms);
        }

        public static int InsertItem(OrderItem item)
        {
            var prms = new SqlParameter[]
            {
                new SqlParameter("@OrderId", item.OrderId),
                new SqlParameter("@ProductId", item.ProductId),
                new SqlParameter("@ProductName", item.ProductName ?? string.Empty),
                new SqlParameter("@ProductSKU", (object?)item.ProductSKU ?? DBNull.Value),
                new SqlParameter("@ProductImage", (object?)item.ProductImage ?? DBNull.Value),
                new SqlParameter("@Price", item.Price),
                new SqlParameter("@Quantity", item.Quantity),
                new SqlParameter("@TotalPrice", item.TotalPrice)
            };
            return BaseConnectionSql.ExecuteNonQuery("SP_OrderItems_Insert", prms);
        }

        public static List<OrderItem> GetItemsByOrderId(int orderId)
        {
            var prms = new SqlParameter[] { new SqlParameter("@OrderId", orderId) };
            return BaseConnectionSql.ExecuteStoredProcedure<OrderItem>("SP_OrderItems_GetByOrderId", prms);
        }

        public static List<OrderStatusHistory> GetHistoryByOrderId(int orderId)
        {
            var prms = new SqlParameter[] { new SqlParameter("@OrderId", orderId) };
            return BaseConnectionSql.ExecuteStoredProcedure<OrderStatusHistory>("SP_OrderStatusHistory_GetByOrderId", prms);
        }

        public static OrderCountModel GetStatusCounts()
        {
            var list = BaseConnectionSql.ExecuteStoredProcedure<OrderCountModel>("SP_Orders_GetStatusCounts", null);
            if (list != null && list.Count > 0) return list[0];
            return new OrderCountModel();
        }
    }
}
