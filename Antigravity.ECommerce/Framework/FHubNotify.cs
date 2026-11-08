using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.SqlClient;
using Antigravity.ECommerce.Models;
using Antigravity.ECommerce.Services;

namespace Antigravity.ECommerce.Framework
{
    public class FHubNotify
    {
        public static int Enqueue(
            string orderCode,
            string json,
            string? customerName,
            string? customerPhone,
            string? customerAddress,
            string? productSku,
            string? productName,
            int quantity,
            string? unit,
            decimal price,
            DateTime? expectedDate,
            string? note)
        {
            var prms = new SqlParameter[]
            {
                new SqlParameter("@OrderCode", orderCode),
                new SqlParameter("@Json", json),
                new SqlParameter("@CustomerName", (object?)customerName ?? DBNull.Value),
                new SqlParameter("@CustomerPhone", (object?)customerPhone ?? DBNull.Value),
                new SqlParameter("@CustomerAddress", (object?)customerAddress ?? DBNull.Value),
                new SqlParameter("@ProductSKU", (object?)productSku ?? DBNull.Value),
                new SqlParameter("@ProductName", (object?)productName ?? DBNull.Value),
                new SqlParameter("@Quantity", quantity > 0 ? quantity : 1),
                new SqlParameter("@Unit", (object?)unit ?? DBNull.Value),
                new SqlParameter("@Price", price),
                new SqlParameter("@ExpectedDate", (object?)expectedDate ?? DBNull.Value),
                new SqlParameter("@Note", (object?)note ?? DBNull.Value)
            };
            return BaseConnectionSql.ExecuteNonQuery("SP_HubNotifyQueue_Enqueue", prms);
        }

        public static int UpdateHubStatus(string orderCode, bool hubSent, string? error)
        {
            var prms = new SqlParameter[]
            {
                new SqlParameter("@OrderCode", orderCode),
                new SqlParameter("@HubSent", hubSent),
                new SqlParameter("@HubLastError", (object?)error ?? DBNull.Value)
            };
            return BaseConnectionSql.ExecuteNonQuery("SP_HubNotifyQueue_UpdateHubStatus", prms);
        }

        public static List<HubNotifyItem> Search(
            string? kw,
            int? status,
            int? hubStatus,
            DateTime? dateMin,
            DateTime? dateMax,
            int page = 1,
            int size = 20)
        {
            var prms = new SqlParameter[]
            {
                new SqlParameter("@Keyword", (object?)kw ?? DBNull.Value),
                new SqlParameter("@Status", (object?)status ?? DBNull.Value),
                new SqlParameter("@HubStatus", (object?)hubStatus ?? DBNull.Value),
                new SqlParameter("@Ngay_Min", (object?)dateMin ?? DBNull.Value),
                new SqlParameter("@Ngay_Max", (object?)dateMax ?? DBNull.Value),
                new SqlParameter("@PageIndex", page),
                new SqlParameter("@PageSize", size)
            };
            return BaseConnectionSql.ExecuteStoredProcedure<HubNotifyItem>("SP_HubNotifyQueue_Search", prms);
        }

        public static HubNotifyStatusCounts GetStatusCounts()
        {
            try
            {
                var list = BaseConnectionSql.ExecuteStoredProcedure<HubNotifyStatusCounts>("SP_HubNotifyQueue_GetStatusCounts", null);
                return list.FirstOrDefault() ?? new HubNotifyStatusCounts();
            }
            catch
            {
                return new HubNotifyStatusCounts();
            }
        }

        public static HubNotifyItem? GetById(int id)
        {
            var prms = new SqlParameter[] { new SqlParameter("@Id", id) };
            return BaseConnectionSql.ExecuteStoredProcedure<HubNotifyItem>("SP_HubNotifyQueue_GetById", prms).FirstOrDefault();
        }

        public static int UpdateStatus(int id, int status, string? note = null)
        {
            var prms = new SqlParameter[]
            {
                new SqlParameter("@Id", id),
                new SqlParameter("@Status", status),
                new SqlParameter("@Note", (object?)note ?? DBNull.Value)
            };
            return BaseConnectionSql.ExecuteNonQuery("SP_HubNotifyQueue_UpdateStatus", prms);
        }

        public static int DeleteById(int id)
        {
            var prms = new SqlParameter[] { new SqlParameter("@Id", id) };
            return BaseConnectionSql.ExecuteNonQuery("SP_HubNotifyQueue_DeleteById", prms);
        }
    }
}

