using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Antigravity.ECommerce.Models;
using Antigravity.ECommerce.Services;

namespace Antigravity.ECommerce.Framework
{
    public class FDashboard
    {
        public static DashboardSummary GetSummary()
        {
            var list = BaseConnectionSql.ExecuteStoredProcedure<DashboardSummary>("SP_Dashboard_Summary", null);
            return list != null && list.Count > 0 ? list[0] : new DashboardSummary();
        }

        public static List<RevenueByDay> GetRevenueByDay(int days)
        {
            var prms = new SqlParameter[] { new SqlParameter("@Days", days) };
            return BaseConnectionSql.ExecuteStoredProcedure<RevenueByDay>("SP_Dashboard_RevenueByDay", prms);
        }

        public static List<TopProduct> GetTopProducts(int top)
        {
            var prms = new SqlParameter[] { new SqlParameter("@Top", top) };
            return BaseConnectionSql.ExecuteStoredProcedure<TopProduct>("SP_Dashboard_TopProducts", prms);
        }

        public static List<TopCategory> GetTopCategories(int top)
        {
            var prms = new SqlParameter[] { new SqlParameter("@Top", top) };
            return BaseConnectionSql.ExecuteStoredProcedure<TopCategory>("SP_Dashboard_TopCategories", prms);
        }

        public static List<Order> GetRecentOrders(int top)
        {
            var prms = new SqlParameter[] { new SqlParameter("@Top", top) };
            return BaseConnectionSql.ExecuteStoredProcedure<Order>("SP_Dashboard_RecentOrders", prms);
        }

        public static List<Product> GetLowStock(int top)
        {
            var prms = new SqlParameter[] { new SqlParameter("@Top", top) };
            return BaseConnectionSql.ExecuteStoredProcedure<Product>("SP_Dashboard_LowStock", prms);
        }
        public static List<ViewsByDay> GetViewsByDay(int days)
        {
            var prms = new SqlParameter[] { new SqlParameter("@Days", days) };
            return BaseConnectionSql.ExecuteStoredProcedure<ViewsByDay>("SP_Dashboard_ViewsByDay", prms);
        }

        public static List<TrafficSource> GetTrafficSource()
        {
            return BaseConnectionSql.ExecuteStoredProcedure<TrafficSource>("SP_Dashboard_TrafficSource", null);
        }
    }
}
