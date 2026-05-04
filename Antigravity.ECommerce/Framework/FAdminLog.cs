using Antigravity.ECommerce.Models;
using Antigravity.ECommerce.Services;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;

namespace Antigravity.ECommerce.Framework
{
    public class FAdminLog
    {
        public static void Insert(AdminLog model)
        {
            var sql = @"INSERT INTO AdminLogs (Username, Action, Module, Description, IpAddress, CreatedAt)
                        VALUES (@Username, @Action, @Module, @Description, @IpAddress, GETDATE())";
            BaseConnectionSql.ExecuteNonQuery(sql, new SqlParameter[]
            {
                new SqlParameter("@Username", (object)model.Username ?? DBNull.Value),
                new SqlParameter("@Action", (object)model.Action ?? DBNull.Value),
                new SqlParameter("@Module", (object)model.Module ?? DBNull.Value),
                new SqlParameter("@Description", (object)model.Description ?? DBNull.Value),
                new SqlParameter("@IpAddress", (object)model.IpAddress ?? DBNull.Value)
            });
        }

        public static List<AdminLog> Search(string module, string username, string action, string fromDate, string toDate, int page, int pageSize)
        {
            var sql = @"SELECT * FROM AdminLogs WHERE 1=1 ";
            var prms = new List<SqlParameter>();
            
            if (!string.IsNullOrEmpty(module)) {
                sql += " AND Module = @Module ";
                prms.Add(new SqlParameter("@Module", module));
            }
            if (!string.IsNullOrEmpty(username)) {
                sql += " AND Username = @Username ";
                prms.Add(new SqlParameter("@Username", username));
            }
            if (!string.IsNullOrEmpty(action)) {
                sql += " AND Action = @Action ";
                prms.Add(new SqlParameter("@Action", action));
            }
            if (!string.IsNullOrEmpty(fromDate) && DateTime.TryParse(fromDate, out DateTime fd)) {
                sql += " AND CreatedAt >= @FromDate ";
                prms.Add(new SqlParameter("@FromDate", fd.Date));
            }
            if (!string.IsNullOrEmpty(toDate) && DateTime.TryParse(toDate, out DateTime td)) {
                sql += " AND CreatedAt < @ToDate ";
                prms.Add(new SqlParameter("@ToDate", td.Date.AddDays(1))); // to end of the day
            }
            
            int offset = (page - 1) * pageSize;
            sql += $" ORDER BY CreatedAt DESC OFFSET {offset} ROWS FETCH NEXT {pageSize} ROWS ONLY";
            
            return BaseConnectionSql.Query<AdminLog>(sql, prms.ToArray());
        }
        
        public static int GetTotalCount(string module, string username, string action, string fromDate, string toDate)
        {
            var sql = @"SELECT COUNT(*) FROM AdminLogs WHERE 1=1 ";
            var prms = new List<SqlParameter>();
            
            if (!string.IsNullOrEmpty(module)) {
                sql += " AND Module = @Module ";
                prms.Add(new SqlParameter("@Module", module));
            }
            if (!string.IsNullOrEmpty(username)) {
                sql += " AND Username = @Username ";
                prms.Add(new SqlParameter("@Username", username));
            }
            if (!string.IsNullOrEmpty(action)) {
                sql += " AND Action = @Action ";
                prms.Add(new SqlParameter("@Action", action));
            }
            if (!string.IsNullOrEmpty(fromDate) && DateTime.TryParse(fromDate, out DateTime fd)) {
                sql += " AND CreatedAt >= @FromDate ";
                prms.Add(new SqlParameter("@FromDate", fd.Date));
            }
            if (!string.IsNullOrEmpty(toDate) && DateTime.TryParse(toDate, out DateTime td)) {
                sql += " AND CreatedAt < @ToDate ";
                prms.Add(new SqlParameter("@ToDate", td.Date.AddDays(1)));
            }
            
            var res = BaseConnectionSql.ExecuteScalar(sql, prms.ToArray());
            return res != null ? Convert.ToInt32(res) : 0;
        }

        public static List<string> GetDistinctModules()
        {
            var list = new List<string>();
            var sql = "SELECT DISTINCT Module FROM AdminLogs WHERE Module IS NOT NULL ORDER BY Module";
            using (var conn = new SqlConnection(BaseConnectionSql.ConnectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(sql, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(reader.GetString(0));
                    }
                }
            }
            return list;
        }

        public static List<string> GetDistinctUsernames()
        {
            var list = new List<string>();
            var sql = "SELECT DISTINCT Username FROM AdminLogs WHERE Username IS NOT NULL ORDER BY Username";
            using (var conn = new SqlConnection(BaseConnectionSql.ConnectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(sql, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(reader.GetString(0));
                    }
                }
            }
            return list;
        }

        public static int ClearAllLogs()
        {
            var sql = "TRUNCATE TABLE AdminLogs";
            return BaseConnectionSql.ExecuteNonQuery(sql, null);
        }

        public static int ClearOldLogs(int daysLimit = 30)
        {
            var sql = "DELETE FROM AdminLogs WHERE CreatedAt < DATEADD(DAY, -@Days, GETDATE())";
            return BaseConnectionSql.ExecuteNonQuery(sql, new SqlParameter[] { new SqlParameter("@Days", daysLimit) });
        }
    }
}
