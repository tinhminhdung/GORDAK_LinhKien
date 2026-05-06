using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.SqlClient;
using Antigravity.ECommerce.Models;
using Antigravity.ECommerce.Framework;

namespace Antigravity.ECommerce.Services
{
    /// <summary> Dá»‹ch vá»¥ quáº£n lÃ½ Quáº£ng cÃ¡o vÃ  Banner </summary>
    public class SAdvertising
    {
        public static List<Advertising> GetAll()
        {
            return SCache.GetOrSet("Advertising_All", () => {
                return BaseConnectionSql.ExecuteStoredProcedure<Advertising>("SP_Advertisings_GetAll", null);
            }) ?? new List<Advertising>();
        }

        public static Advertising? GetById(int id)
        {
            var prm = new SqlParameter[] { new SqlParameter("@Id", id) };
            var list = BaseConnectionSql.ExecuteStoredProcedure<Advertising>("SP_Advertisings_GetById", prm);
            return list != null && list.Count > 0 ? list[0] : null;
        }

        public static List<Advertising> GetByPosition(string position)
        {
            var all = GetAll();
            var now = DateTime.Now;
            return all.FindAll(x => x.Position == position && x.Status == 1 
                                && (x.StartDate == null || x.StartDate <= now)
                                && (x.EndDate == null || x.EndDate >= now))
                      .OrderBy(x => x.SortOrder).ToList();
        }

        public static int Insert(Advertising obj)
        {
            var prm = new SqlParameter[] {
                new SqlParameter("@Title", obj.Title ?? ""),
                new SqlParameter("@Image", (object?)obj.Image ?? DBNull.Value),
                new SqlParameter("@VideoUrl", (object?)obj.VideoUrl ?? DBNull.Value),
                new SqlParameter("@Link", (object?)obj.Link ?? DBNull.Value),
                new SqlParameter("@Description", (object?)obj.Description ?? DBNull.Value),
                new SqlParameter("@Position", (object?)obj.Position ?? DBNull.Value),
                new SqlParameter("@SortOrder", obj.SortOrder),
                new SqlParameter("@Status", obj.Status),
                new SqlParameter("@Target", (object?)obj.Target ?? "_self"),
                new SqlParameter("@StartDate", (object?)obj.StartDate ?? DBNull.Value),
                new SqlParameter("@EndDate", (object?)obj.EndDate ?? DBNull.Value),
                new SqlParameter("@CreatedBy", (object?)obj.CreatedBy ?? DBNull.Value)
            };
            var result = BaseConnectionSql.ExecuteScalar("SP_Advertisings_Insert", prm);
            int id = result != null ? Convert.ToInt32(result) : 0;
            if (id > 0) SCache.Remove("Advertising_All");
            return id;
        }

        public static int Update(Advertising obj)
        {
            var prm = GetParameters(obj);
            var result = BaseConnectionSql.ExecuteNonQuery("SP_Advertisings_Update", prm);
            if (result > 0) SCache.Remove("Advertising_All");
            return result;
        }

        public static int Delete(int id)
        {
            var prm = new SqlParameter[] { new SqlParameter("@Id", id) };
            var result = BaseConnectionSql.ExecuteNonQuery("SP_Advertisings_Delete", prm);
            if (result > 0) SCache.Remove("Advertising_All");
            return result;
        }

        public static List<Advertising> Search(string kw, string position, int? status, string sort = "CreatedAt", string order = "DESC", int page = 1, int size = 20)
        {
            var prms = new SqlParameter[] {
                new SqlParameter("@Keyword", string.IsNullOrEmpty(kw) ? DBNull.Value : kw),
                new SqlParameter("@Position", string.IsNullOrEmpty(position) ? DBNull.Value : position),
                new SqlParameter("@Status", (object?)status ?? DBNull.Value),
                new SqlParameter("@SortColumn", sort ?? "CreatedAt"),
                new SqlParameter("@SortOrder", order ?? "DESC"),
                new SqlParameter("@PageIndex", page),
                new SqlParameter("@PageSize", size)
            };
            
            return BaseConnectionSql.ExecuteStoredProcedure<Advertising>("SP_Advertisings_Search", prms);
        }

        public static int BulkDelete(List<int> ids)
        {
            if (ids == null || ids.Count == 0) return 0;
            int count = 0;
            foreach (var id in ids)
            {
                count += Delete(id);
            }
            return count;
        }

        public static int BulkUpdateStatus(List<int> ids, int status)
        {
            if (ids == null || ids.Count == 0) return 0;
            int count = 0;
            foreach (var id in ids)
            {
                count += BaseConnectionSql.ExecuteNonQuery("UPDATE Advertisings SET Status = @Status WHERE AdvertisingId = @Id", 
                    new SqlParameter("@Status", status), new SqlParameter("@Id", id));
            }
            if (count > 0) SCache.Remove("Advertising_All");
            return count;
        }

        private static SqlParameter[] GetParameters(Advertising obj)
        {
            return new SqlParameter[] {
                new SqlParameter("@AdvertisingId", obj.AdvertisingId),
                new SqlParameter("@Title", obj.Title ?? ""),
                new SqlParameter("@Image", (object?)obj.Image ?? DBNull.Value),
                new SqlParameter("@VideoUrl", (object?)obj.VideoUrl ?? DBNull.Value),
                new SqlParameter("@Link", (object?)obj.Link ?? DBNull.Value),
                new SqlParameter("@Description", (object?)obj.Description ?? DBNull.Value),
                new SqlParameter("@Position", (object?)obj.Position ?? DBNull.Value),
                new SqlParameter("@SortOrder", obj.SortOrder),
                new SqlParameter("@Status", obj.Status),
                new SqlParameter("@Target", (object?)obj.Target ?? "_self"),
                new SqlParameter("@StartDate", (object?)obj.StartDate ?? DBNull.Value),
                new SqlParameter("@EndDate", (object?)obj.EndDate ?? DBNull.Value),
                new SqlParameter("@UpdatedBy", (object?)obj.UpdatedBy ?? DBNull.Value)
            };
        }
    }
}

