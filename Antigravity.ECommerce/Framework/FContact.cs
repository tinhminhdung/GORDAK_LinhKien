using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Antigravity.ECommerce.Models;
using Antigravity.ECommerce.Services;

namespace Antigravity.ECommerce.Framework
{
    public class ContactStats
    {
        public int TotalCount { get; set; }
        public int UnreadCount { get; set; }
        public int TodayCount { get; set; }
        public int WeekCount { get; set; }
    }

    public class FContact
    {
        public static ContactMessage? GetById(int id)
        {
            var prm = new SqlParameter[] { new SqlParameter("@ContactId", id) };
            var list = BaseConnectionSql.ExecuteStoredProcedure<ContactMessage>("SP_ContactMessages_GetById", prm);
            return list != null && list.Count > 0 ? list[0] : null;
        }

        public static List<ContactMessage> Search(string? keyword, int? status, string? dateFilter, string sortColumn, string sortOrder, int pageIndex, int pageSize)
        {
            var prm = new SqlParameter[]
            {
                new SqlParameter("@Keyword", (object?)keyword ?? DBNull.Value),
                new SqlParameter("@Status", (object?)status ?? DBNull.Value),
                new SqlParameter("@DateFilter", (object?)dateFilter ?? DBNull.Value),
                new SqlParameter("@SortColumn", sortColumn),
                new SqlParameter("@SortOrder", sortOrder),
                
                new SqlParameter("@PageIndex", pageIndex),
                new SqlParameter("@PageSize", pageSize)
            };
            return BaseConnectionSql.ExecuteStoredProcedure<ContactMessage>("SP_ContactMessages_Search", prm);
        }

        public static int Insert(ContactMessage obj)
        {
            var prm = new SqlParameter[]
            {
                new SqlParameter("@FullName", obj.FullName),
                new SqlParameter("@Email", obj.Email),
                new SqlParameter("@Phone", (object?)obj.Phone ?? DBNull.Value),
                new SqlParameter("@Subject", (object?)obj.Subject ?? DBNull.Value),
                new SqlParameter("@Message", obj.Message),
                new SqlParameter("@Status", obj.Status),
                new SqlParameter("@IsStarred", obj.IsStarred)
            };
            var result = BaseConnectionSql.ExecuteScalar("SP_ContactMessages_Insert", prm);
            return result != null ? Convert.ToInt32(result) : 0;
        }

        public static int Update(ContactMessage obj)
        {
            var prm = new SqlParameter[]
            {
                new SqlParameter("@ContactId", obj.ContactId),
                new SqlParameter("@FullName", obj.FullName),
                new SqlParameter("@Email", obj.Email),
                new SqlParameter("@Phone", (object?)obj.Phone ?? DBNull.Value),
                new SqlParameter("@Subject", (object?)obj.Subject ?? DBNull.Value),
                new SqlParameter("@Message", obj.Message),
                new SqlParameter("@Status", obj.Status),
                new SqlParameter("@IsStarred", obj.IsStarred),
                new SqlParameter("@AdminNote", (object?)obj.AdminNote ?? DBNull.Value),
                new SqlParameter("@ReadAt", (object?)obj.ReadAt ?? DBNull.Value),
                new SqlParameter("@UpdatedBy", (object?)obj.UpdatedBy ?? DBNull.Value)
            };
            return BaseConnectionSql.ExecuteNonQuery("SP_ContactMessages_Update", prm);
        }

        public static int Delete(int id)
        {
            var prm = new SqlParameter[] { new SqlParameter("@ContactId", id) };
            return BaseConnectionSql.ExecuteNonQuery("SP_ContactMessages_Delete", prm);
        }

        public static int GetTotalCount(string? keyword, int? status, string? dateFilter)
        {
            var prm = new SqlParameter[]
            {
                new SqlParameter("@Keyword", (object?)keyword ?? DBNull.Value),
                new SqlParameter("@Status", (object?)status ?? DBNull.Value),
                new SqlParameter("@DateFilter", (object?)dateFilter ?? DBNull.Value)
            };
            var result = BaseConnectionSql.ExecuteScalar("SP_ContactMessages_GetTotalCount", prm);
            return result != null ? Convert.ToInt32(result) : 0;
        }

        public static ContactStats GetStats()
        {
            // Tráº£ vá» thá»‘ng kÃª tÃ³m táº¯t
            var list = BaseConnectionSql.ExecuteStoredProcedure<ContactStats>("SP_ContactMessages_GetStats", null);
            return list != null && list.Count > 0 ? list[0] : new ContactStats { TotalCount = 0, UnreadCount = 0, TodayCount = 0, WeekCount = 0 };
        }
    }
}


