using System;
using System.Collections.Generic;
using Antigravity.ECommerce.Models;
using Antigravity.ECommerce.Framework;

namespace Antigravity.ECommerce.Services
{
    public class SDashboard
    {
        public static DashboardSummary GetSummary() => FDashboard.GetSummary();
        public static List<RevenueByDay> GetRevenueByDay(int days) => FDashboard.GetRevenueByDay(days);
        public static List<TopProduct> GetTopProducts(int top) => FDashboard.GetTopProducts(top);
        public static List<TopCategory> GetTopCategories(int top) => FDashboard.GetTopCategories(top);
        public static List<Order> GetRecentOrders(int top) => FDashboard.GetRecentOrders(top);
        public static List<Product> GetLowStock(int top) => FDashboard.GetLowStock(top);
        public static List<ViewsByDay> GetViewsByDay(int days) => FDashboard.GetViewsByDay(days);
        public static List<TrafficSource> GetTrafficSource() => FDashboard.GetTrafficSource();
    }
}
