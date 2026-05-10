using System;
using System.Collections.Generic;
using Antigravity.ECommerce.Models;
using Antigravity.ECommerce.Framework;

namespace Antigravity.ECommerce.Services
{
    public class SOrder
    {
        public static int Insert(Order order, List<OrderItem> items)
        {
            // Calculate totals if not already
            decimal subTotal = 0;
            foreach (var item in items)
            {
                subTotal += item.TotalPrice;
            }
            order.SubTotal = subTotal;
            order.TotalAmount = subTotal + order.ShippingFee - order.Discount;
            
            // Default status
            order.OrderStatus = 0; // New

            // Insert Order
            int orderId = FOrder.Insert(order);

            // Insert Items and deduct stock
            foreach (var item in items)
            {
                item.OrderId = orderId;
                FOrder.InsertItem(item);

                // Deduct stock
                var product = SProduct.GetById(item.ProductId);
                if (product != null)
                {
                    product.Stock = Math.Max(0, product.Stock - item.Quantity);
                    product.Sales += item.Quantity;
                    FProduct.Update(product);
                }
            }
            // Sync Customer Rank since a new order affects TotalOrders and TotalSpent
            if (order.CustomerId != null && order.CustomerId > 0)
            {
                SCustomer.RefreshRank(order.CustomerId.Value);
            }

            return orderId;
        }

        public static int Update(Order order)
        {
            return FOrder.Update(order);
        }

        public static int UpdateStatus(int orderId, int newStatus, string? trackingCode, string? note, string? updatedBy)
        {
            var oldOrder = GetById(orderId);
            if (oldOrder == null || oldOrder.OrderStatus == newStatus) return 0;
            
            int result = FOrder.UpdateStatus(orderId, newStatus, trackingCode, note, updatedBy);

            // Handle cancellation logic to restore inventory
            if (newStatus == 4 || newStatus == 5) // Cancelled or Returned
            {
                var items = GetItemsByOrderId(orderId);
                foreach (var item in items)
                {
                    var product = SProduct.GetById(item.ProductId);
                    if (product != null)
                    {
                        product.Stock += item.Quantity;
                        product.Sales = Math.Max(0, product.Sales - item.Quantity);
                        FProduct.Update(product);
                    }
                }
            }

            // Đồng bộ lại chi tiêu và thứ hạng khách hàng cho mọi thay đổi trạng thái
            if (oldOrder.CustomerId != null && oldOrder.CustomerId > 0)
            {
                SCustomer.RefreshRank(oldOrder.CustomerId.Value);
            }

            return result;
        }

        public static List<Order> Search(string kw, int? status, int? paymentStatus, int? provinceId, int? wardId, bool? requiresVat, DateTime? dateMin, DateTime? dateMax, string sort, string order, int page, int size)
        {
            return FOrder.Search(kw, status, paymentStatus, provinceId, wardId, requiresVat, dateMin, dateMax, sort, order, page, size);
        }

        public static Order? GetById(int orderId)
        {
            return FOrder.GetById(orderId);
        }

        public static int Delete(int orderId)
        {
            // Explicitly delete related data to ensure integrity and fulfill "delete everything related" requirement
            BaseConnectionSql.ExecuteNonQuery("DELETE FROM OrderItems WHERE OrderId = @id", new Microsoft.Data.SqlClient.SqlParameter("@id", orderId));
            BaseConnectionSql.ExecuteNonQuery("DELETE FROM OrderStatusHistory WHERE OrderId = @id", new Microsoft.Data.SqlClient.SqlParameter("@id", orderId));
            
            return FOrder.Delete(orderId);
        }

        public static void BulkDelete(List<int> ids)
        {
            if (ids == null || ids.Count == 0) return;
            foreach (var id in ids)
            {
                Delete(id);
            }
        }

        public static List<OrderItem> GetItemsByOrderId(int orderId)
        {
            return FOrder.GetItemsByOrderId(orderId);
        }

        public static List<OrderStatusHistory> GetHistoryByOrderId(int orderId)
        {
            return FOrder.GetHistoryByOrderId(orderId);
        }

        public static OrderCountModel GetStatusCounts()
        {
            return FOrder.GetStatusCounts();
        }
    }
}
