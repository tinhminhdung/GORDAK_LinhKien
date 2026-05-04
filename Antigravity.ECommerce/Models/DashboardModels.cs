using System;
using System.Collections.Generic;

namespace Antigravity.ECommerce.Models
{
    public class DashboardSummary
    {
        public decimal TodayRevenue { get; set; }
        public decimal YesterdayRevenue { get; set; }
        public int NewOrderCount { get; set; }
        public int ProcessingOrderCount { get; set; }
        public int NewCustomerCount7d { get; set; }
        public int NewCustomerCountPrev7d { get; set; }
        public int LowStockCount { get; set; }

        public int OnlineUsers { get; set; }
        public int TotalViewsToday { get; set; }

        public decimal RevenueGrowth => YesterdayRevenue == 0 
            ? (TodayRevenue > 0 ? 100 : 0) 
            : Math.Round((TodayRevenue - YesterdayRevenue) / YesterdayRevenue * 100, 1);
        
        public decimal CustomerGrowth => NewCustomerCountPrev7d == 0 
            ? (NewCustomerCount7d > 0 ? 100 : 0) 
            : Math.Round((decimal)(NewCustomerCount7d - NewCustomerCountPrev7d) / NewCustomerCountPrev7d * 100, 1);
    }

    public class ViewsByDay
    {
        public DateTime Date { get; set; }
        public int Views { get; set; }
    }

    public class TrafficSource
    {
        public string Source { get; set; } = string.Empty;
        public int TotalViews { get; set; }
    }

    public class RevenueByDay
    {
        public DateTime Date { get; set; }
        public decimal Revenue { get; set; }
        public int OrderCount { get; set; }
    }

    public class TopProduct
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? MainImage { get; set; }
        public int TotalSold { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class TopCategory
    {
        public int CategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal TotalRevenue { get; set; }
        public int ProductCount { get; set; }
    }
}
